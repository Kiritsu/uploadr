namespace ShareY.Database
{
    public interface IDatabaseConfigurationProvider
    {
        IDatabaseConfiguration GetConfiguration();
    }
}
