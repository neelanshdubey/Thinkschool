namespace QuotesApi.Services;

public class JwtSettings
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Key { get; set; }
    public int ExpiresIn { get; set; } = 3600;
}
