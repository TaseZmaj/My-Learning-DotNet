using System.Text;
using System.Threading.RateLimiting;
using EventsManagement.Domain.Configuration;
using EventsManagement.Repository.Implementations;
using EvolveDb;
using EventsManagement.Repository;
using EventsManagement.Repository.Interfaces;
using EventsManagement.Domain.Entities;
using EventsManagement.Web.Interceptor;
using EventsManagement.Web.Mapper;
using EventsManagement.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Quartz;
using Service.Clients;
using Service.Implementations;
using Service.Interfaces;
using Service.jobs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Logging.AddConsole(); //for debugging

builder.Services.AddScoped<AuditInterceptor>();
//MAIN DATABASE CONTEXT ===============================================
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.UseLazyLoadingProxies();
   
    var interceptor = sp.GetRequiredService<AuditInterceptor>();
    options.AddInterceptors(interceptor);
    
    // sp.GetService<AuditInterceptor>();
});
// ====================================================================

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentity<EventsAppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRazorPages();

//REPOSITORIES
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<ILegacyVenueRepository, LegacyVenueRepository>();

//SERVICES
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<VenueETLService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

//Inbound Service - Aud 8 =============================================================
builder.Services.AddScoped<IInboundEventService, InboundEventService>();
builder.Services.AddScoped<IInboundEventEntryProcessor, InboundEventEntryProcessor>();
// =====================================================================================

builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429; //429 - Too many requests

    options.AddPolicy("external-api", context =>
    {
        var apiKey = context.Request.Headers["X-API-key"];

        var apiClient = context.Items["ApiClient"] as ApiClient;

        return RateLimitPartition.GetFixedWindowLimiter(apiKey.ToString(), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});

//WEATHER API TYPED CLIENT SERVICE
//========================================================================================================
    //Explanation: Linijava kod go gleda appsettings.json i vo nego go gleda "WeatherApi":{}
    //od tuka, gi plugnuva-in vrednostite.  Gemini:Every time the Dependency Injection (DI)
    //system creates an instance of a class that requests those settings, it will "inject"
    //the values from your appsettings.json.
    //Ova se narekuva CONFIGURATION BINDING.
builder.Services.Configure<WeatherApiSettings>(
    builder.Configuration.GetSection("WeatherApi"));

    //REGISTRIRANJE NOV HTTPCLIENT (WeatherService klasata go koristi)
builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<WeatherApiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
    // client.DefaultRequestHeaders.Add("", settings.ApiKey); //Vaka ke go plugneshe-in API key-ot ako trebase
    // da bide vo Headers, no moze da e i kako del od request-ot.  Zavisi od API-to
     
    // client.BaseAddress = new Uri("https://api.openweathermap.org/"); //NE e preporaclivo vaka da go pravish
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromSeconds(2);
    options.Retry.BackoffType =
        DelayBackoffType.Exponential;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
});
// ===========================================================================================================

//EXTERNAL EVENTS API TYPED CLIENT SETUP - with OAuth2 =======================================================
//OAuth2 Token service
//go nemashe ova vo Auditoriski kod
builder.Services.AddHttpClient<TokenService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ExternalEventApiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
}); 

builder.Services.AddScoped<TokenService>();

builder.Services.Configure<ExternalEventApiSettings>(
    builder.Configuration.GetSection("ExternalEventApi"));

builder.Services.AddHttpClient<ExternalEventApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ExternalEventApiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
    //Prilicno siguren sum deka nekako treba da gi plugnesh-in i ostanatite vrednosti
    //od appsettings.json, samo nz kako
});
// ==========================================================================================================
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

//MAPPERS
builder.Services.AddScoped<EventMapper>();
builder.Services.AddScoped<ReservationMapper>();

//BACKGROUND SERVICES =========================================================================
builder.Services.AddHostedService<ReservationCleanupBackgroundService>();
builder.Services.AddHostedService<LegacyDBEtlBackgroundService>();

builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    
    // AUD 6 -> Job 1: Reservation Cleanup
    var cleanupKey = new JobKey("reservation-cleanup", "maintenance");
    options.AddJob<QuartzReservationCleanupJob>(o => o.WithIdentity(cleanupKey));
    options.AddTrigger(o => o
        .ForJob(cleanupKey)
        .WithIdentity("reservation-cleanup-trigger")
        .WithCronSchedule("0/30 * * * * ?"));

    // AUD 8 -> Job 2: Inbound Event Processing
    var inboundKey = new JobKey("inbound-event-entry-batch-save", "maintenance");
    options.AddJob<InboundEventProcessingJob>(o => o.WithIdentity(inboundKey));
    options.AddTrigger(o => o
        .ForJob(inboundKey)
        .WithIdentity("inbound-event-entry-batch-save-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(20).RepeatForever()));
        // .WithCronSchedule("0/20 * * * * ?") // Every 20 seconds for testing
        // .WithDescription("Enters Pending inbound events into the database in batches of 10"));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
//=============================================================================================

//LEGACY DB CONNECTION ========================================================================
builder.Services.AddDbContext<LegacyApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("LegacyVenueDb")));
// ============================================================================================


// JWT ==============================================================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]
                                 ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });
//===================================================================================


//============================ KOD ZA evolve ========================================
// try
// {
//     using var cnx = new SqlConnection(connectionString);
//
//     var evolve = new Evolve(cnx, msg => Console.WriteLine(msg))
//     {
//         Locations = new[] { "Database/Migrations" },
//         IsEraseDisabled = true,
//         OutOfOrder = true
//     };
//
//     evolve.Migrate();
// }
// catch (Exception ex)
// {
//     Console.WriteLine("Migration failed");
//     Console.WriteLine(ex);
//     throw;
// }
//=================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// MIDDLEWARE =======================================================================
app.UseMiddleware<ApiKeyMiddleware>();
// ==================================================================================

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();