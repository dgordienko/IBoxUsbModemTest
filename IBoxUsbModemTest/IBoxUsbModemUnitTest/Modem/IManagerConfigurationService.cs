namespace IBoxUsbModemUnitTest.Modem
{
    public interface IManagerConfigurationService<T> : IDisposableService
    {
        T LoadConfiguration();
        void SaveConfiguration(T configuration);
    }
}
