namespace UploadR.Database
{
    public interface IDatabaseConfiguration
    {
        string Hostname { get; set; }
        int Port { get; set; }
        string Database { get; set; }
        string Username { get; set; }
        string Password { get; set; }
    }
}
