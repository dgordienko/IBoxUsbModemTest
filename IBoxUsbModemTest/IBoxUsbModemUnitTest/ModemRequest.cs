using System;
using IBoxUsbModemUnitTest.Modem;


namespace IBoxUsbModemUnitTest
{
    public class ModemRequest
    {
        public ModemStatus Response { get; set; }
        public ConnectConfiguration Connection { get; set; }
        public Exception Exception { get; set; }

    }
}
