using System.Threading.RateLimiting;
using Domain.Configuration;
using Domain.Dto;
using Domain.Models;
using EvolveDb;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repository;
using Repository.Implementation;
using Repository.Interface;
using Service.Implementation;
using Service.Interface;
using Service.Jobs;
using Web.DbSeeder;
using Web.Mapper;
using Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.UseLazyLoadingProxies();
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IConsultationsRepository, ConsultationsRepository>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ConsultationMapper>();
builder.Services.AddScoped<AttendanceMapper>();
builder.Services.AddScoped<IInboundEventEntryProcessor, InboundEventEntryProcessor>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IEtlSyncService, EtlSyncService>();
builder.Services.AddScoped<IInboundEventEntryService, InboundEventEntryService>();

builder.Services.AddHostedService<EtlSyncBackgroundService>();
builder.Services.AddHostedService<AttendanceInboundBackgroundService>();


builder.Services.AddIdentity<ConsultationsApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<ConsultationsApiSettings>(builder.Configuration.GetSection("ConsultationsApi"));
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimitSettings"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection("ApiKeySettings"));

builder.Services.AddHttpClient<IConsultationsApiClient<ExternalConsultationsDto>, ConsultationsApiClient>((sp, client) =>
{

    var settings = sp.GetRequiredService<IOptions<ConsultationsApiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseAddress);
    client.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
    // client.Timeout = TimeSpan.FromSeconds(settings.Value.TimeoutSeconds);
});


builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("external-api", context =>
    {
        var apiKey = context.Request.Headers["X-Api-Key"];

        var apiClient = context.Items["ApiClient"] as ApiClient;

        var settings = context.RequestServices.GetRequiredService<IOptions<RateLimitSettings>>().Value;
        
        return RateLimitPartition.GetFixedWindowLimiter(apiKey.ToString(), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.PermitLimit,
            Window = TimeSpan.FromMinutes(settings.WindowInSeconds),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});



using var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var logger = loggerFactory.CreateLogger("Evolve");

try
{
    using var cnx = new SqliteConnection(connectionString);

    var evolve = new Evolve(cnx, msg => logger.LogInformation(msg))
    {
        Locations = new[] { "Database/Migrations" },
        IsEraseDisabled = true
    };

    evolve.Migrate();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database migration failed.");
    throw;
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ConsultationsApplicationUser>>();

    await DbSeeder.SeedAsync(context, userManager);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }