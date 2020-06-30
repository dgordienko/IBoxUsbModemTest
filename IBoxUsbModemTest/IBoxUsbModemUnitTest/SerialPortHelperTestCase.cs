using System.ComponentModel.Design;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
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
            //.WriteTo.File("SerialPortHelperTestCase.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Console().CreateLogger();


        private  ModemStatus _status = new ModemStatus
        {
            IsSuccess = false,
            State = "Not initialized",
            Manufacturer = "None",
            ModelName = string.Empty,
            SerialNumber = "SN xyz",
            SignalQuality = new SignalQuality { dBmW = 0, Percent = "0.0", IsValid = false },
            Imsi = string.Empty,
            Imei = string.Empty,
            OperatorName = string.Empty
        };

        private readonly ConnectConfiguration _configuration = new ConnectConfiguration
        {
            ConnectionType = ConnectionType.Usb,
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
            PortName = ConfigurationManager.AppSettings["port"]
        };

        [Theory(DisplayName = "Send commands to serial port (ThreadModel)")]
        [InlineData("ATE")]
        [InlineData("ATZ")]
        [InlineData("AT+GSN")]
        [InlineData("AT+GMR")]
        [InlineData("AT+GMI")]
        [InlineData("AT+CGMM")]
        public void SerialPortHelperTest(string command)
        {
            _logger.Information($"send {command}");
            var helper = ModemSerialPort.Instance;
            var recived = false;
            helper.OnSerialPortOpened += (sender, e) =>
            {
                _logger.Information($"call OnSerialPortOpened event; port={_configuration.PortName}");
                sender.Should().BeOfType<ModemSerialPort>();
            };

            helper.OnStatusChanged += (sender, e) =>
            {
                _logger.Information($"call OnStatusChanged event; port={_configuration.PortName}");
                sender.Should().BeOfType<ModemSerialPort>();
            };
            helper.OnDataReceived += (sender, e) =>
            {
                _logger.Information($"call OnDataReceived event; port={_configuration.PortName}");
                sender.Should().BeOfType<ModemSerialPort>();
                e.Should().NotBeNullOrWhiteSpace();
                e.Should().NotBeNullOrEmpty();
                recived = false;
                (sender as ModemSerialPort)?.Close();

            };

            helper.Should().NotBeNull();
            if (!helper.IsOpen)
            {
                //var portName = $"/dev/{_configuration.PortName}";
                var portName = $"{_configuration.PortName}";
                var baudRate = _configuration.BaudRate;
                helper.Open(portname: portName, baudrate: baudRate);
                helper.SendString(command);
                recived = true;
                while (recived)
                {
                    Thread.Sleep(300);
                }
            }
        }

        [Fact(DisplayName = "ATE Command Handler")]        
        public void EchoHandlerTest()
        {
            var request = new ModemRequest
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new EchoHandler();
            var result = handler.Handel(request, "AT");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName ="ATZ Command handler")]
        public void ResetHandlerTest()
        {
            var request = new ModemRequest
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new ResetHandler();
            var result = handler.Handel(request, "ATZ");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "IMSI Command handler")]
        public void ImsiHandlerTest()
        {
            var request = new ModemRequest
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new ImsiHandler();
            var result = handler.Handel(request, "AT+CIMI");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }
    }
}
