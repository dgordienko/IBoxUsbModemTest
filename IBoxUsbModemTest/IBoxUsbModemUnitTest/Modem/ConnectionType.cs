using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IBoxUsbModemUnitTest")]
namespace IBoxUsbModemUnitTest.Modem
{
    public enum ConnectionType
    {
        Ethernet = 0, // local Ethernet
        Gprs = 1,  // modem GPRS
        Usb = 2 // USB 3G module 
    }
}
