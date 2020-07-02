using System.Collections.Generic;

namespace IBox.Modem.IRZ.Shell
{
    public interface IModem : IAdapter
    {
        List<int> AvailableBaudRates { get; }
        ModemStatus FailedModemStatus { get; }

        void SetPort(string portName, int baudeRate);
        bool ApplyConfiguration(ConnectConfiguration configuration);
        ModemStatus GetModemStatus();
    }
}
