using System.Configuration;
using FluentAssertions;
using IBoxUsbModemUnitTest.Modem;
using Serilog;
using Serilog.Core;
using Xunit;


namespace IBoxUsbModemUnitTest
{
    public class SerialPortHelperTestCase
    {
        private readonly Logger _logger = new LoggerConfiguration()
            .WriteTo.File("SerialPortHelperTestCase.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Console().CreateLogger();

        private readonly ConnectConfiguration _configuration = new ConnectConfiguration
        {
            ConnectionType = ConnectionType.Usb,
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
            PortName = ConfigurationManager.AppSettings["port"]
        };

        [Theory(DisplayName ="Send commands to serial port (ThreadModel)")]
        [InlineData("ATE")]
        [InlineData("ATZ")]
        [InlineData("AT+GSN")]
        [InlineData("AT+GMR")]
        [InlineData("AT+GMI")]
        [InlineData("AT+CGMM")]        
        public void SerialPortHelperTest(string command) {
            _logger.Information($"send {command}");
            var helper = ModemSerialPort.Instance;

            helper.OnSerialPortOpened += (sender, e) => {
                _logger.Information($"call OnSerialPortOpened event; port={_configuration.PortName}");
                sender.Should().BeOfType<ModemSerialPort>();
            };

            helper.OnStatusChanged += (sender, e) => {
                _logger.Information($"call OnStatusChanged event; port={_configuration.PortName}");
                sender.Should().BeOfType<ModemSerialPort>();
            };
            helper.OnDataReceived += (sender, e) =>
            {
                _logger.Information($"call OnDataReceived event; port={_configuration.PortName}");
                sender.Should().BeOfType<ModemSerialPort>();
                e.Should().NotBeNullOrWhiteSpace();
                e.Should().NotBeNullOrEmpty();
                (sender as ModemSerialPort)?.Close();
            };

            helper.Should().NotBeNull();
            if (!helper.IsOpen)
            {
                var portName = $"/dev/{_configuration.PortName}";
                var baudRate = _configuration.BaudRate;
                helper.Open(portname: portName,baudrate:baudRate);             
                helper.SendString(command);
            }
        }
    }
}
