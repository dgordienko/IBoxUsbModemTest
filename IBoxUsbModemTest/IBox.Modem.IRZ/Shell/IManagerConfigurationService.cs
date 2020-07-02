namespace IBox.Modem.IRZ.Shell
{
    public interface IManagerConfigurationService<T> : IDisposableService
    {
        T LoadConfiguration();
        void SaveConfiguration(T configuration);
    }
}
