namespace Hash.Data.Settings;

public class RabbitMqSettings
{
    public int ConcurrencyLimit { get; set; }
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}