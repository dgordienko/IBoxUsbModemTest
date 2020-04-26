
using FluentAssertions;
using IBoxUsbModemUnitTest.Modem;
using Serilog;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    public class UsbModemTestCase
    {
        [Theory(Name ="Test USB Modem")]
        [InlineData("/dev/ttyS0", 115200)]
        public void TestConnectSerial(string port, int baudrate)
        {
            var logger = new LoggerConfiguration().
                    WriteTo.Console().CreateLogger();
            var configuration = new ConnectConfiguration
            {
                ConnectionType = ConnectionType.Usb3G,
                BaudRate = baudrate,
                PortName = port
            };
            configuration.Should().NotBeNull();
            var modem = new Modem.Modem(logger, configuration);
            modem.Should().NotBeNull();
        }
    }
}
