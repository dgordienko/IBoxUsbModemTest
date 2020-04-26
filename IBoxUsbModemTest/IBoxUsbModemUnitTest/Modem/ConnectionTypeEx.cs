using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace IBoxUsbModemUnitTest.Modem
{
    public static class ConnectionTypeEx
    {
        public static int ToMonikId(this ConnectionType self) { return (int)self; }
        private static IList<string> _ethernetControllerClassNames = null;

        static ConnectionTypeEx()
        {
            //if (IBox.Client.Common.HostEnvironment.IsLinux)
            try
            {
                _ethernetControllerClassNames = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet).Select(x => x.Name).ToList();
                ShellConsole.WriteError("Default ethernet: " + string.Join(", ", _ethernetControllerClassNames));
#if DEBUG
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    //if ((networkInterface.OperationalStatus == OperationalStatus.Up) &&
                    //    (networkInterface.NetworkInterfaceType == type))
                    {
                        var ipProperties = networkInterface.GetIPProperties();
                        var dnsAddresses = ipProperties.DnsAddresses;
                        ShellConsole.WriteInfo("Network: Type=[{0}] Name=[{1}] Status=[{2}]; DnsEnabled=[{3}]; dns=[{4}]", networkInterface.NetworkInterfaceType, networkInterface.Name, networkInterface.OperationalStatus, ipProperties.IsDnsEnabled, string.Join("; ", dnsAddresses.Select(x => x.ToString()).ToArray()));
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                ShellConsole.WriteError("ConnectionTypeEx::_ctor: " + ex);
            }
        }

        public static void SetNetworkInterfaceName(this ConnectionType self, IEnumerable<string> classNames)
        {
            if (self == ConnectionType.Ethernet)
                _ethernetControllerClassNames = classNames.ToList();
        }

        public static string GetNetworkInterfaceName(this ConnectionType self)
        {
            switch (self)
            {
                case ConnectionType.Ethernet:
                    return ((_ethernetControllerClassNames == null) || !_ethernetControllerClassNames.Any())
                               ? "enp3s0" // "eth0" //
                               : _ethernetControllerClassNames.First();
                case ConnectionType.Gprs: return "ppp0";
            }
            throw new ArgumentOutOfRangeException("Добавь case..");
        }

        public static string GetInfo(this ConnectionType self)
        {
            switch (self)
            {
                case ConnectionType.Ethernet: return "Ethernet";
                case ConnectionType.Gprs: return "GPRS";
            }
            throw new ArgumentOutOfRangeException("Добавь case..");
        }

        public static ConnectionType Parse(string val)
        {
            foreach (ConnectionType e in Enum.GetValues(typeof(ConnectionType)))
                if ((e.GetNetworkInterfaceName().Equals(val, StringComparison.OrdinalIgnoreCase)) ||
                    (e.GetInfo().Equals(val, StringComparison.OrdinalIgnoreCase)) ||
                    (e.ToString().Equals(val, StringComparison.OrdinalIgnoreCase)))
                    return e;
            throw new ArgumentException(string.Format("Unknown value '{0}'", val));
        }

        /// <summary> Локалка? </summary>
        public static bool IsEthernet(this ConnectionType self) { return self == ConnectionType.Ethernet; }
        /// <summary> Модемка? </summary>
        public static bool IsGprs(this ConnectionType self) { return self == ConnectionType.Gprs; }
    }
}
