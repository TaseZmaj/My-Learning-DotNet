namespace Domain.Configuration;

public class ConsultationsApiSettings
{
    public string BaseAddress { get; set; }
    
    public string ApiKey { get; set; }
}

public class CacheSettings
{
    public int ListCacheDurationMinutes { get; set; }
    public int DetailCacheDurationMinutes { get; set; }
}

public class RateLimitSettings
{
    public int PermitLimit { get; set; }
    public int WindowInSeconds{ get; set; }
    public bool Apply { get; set; }
}

public class ApiKeySettings
{
    public string ApiKey { get; set; }
}