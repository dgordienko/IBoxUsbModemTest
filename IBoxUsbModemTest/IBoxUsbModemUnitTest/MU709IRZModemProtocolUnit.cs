using System.Configuration;
using FluentAssertions;
using Serilog;
using Serilog.Core;
using Xunit;

using IBox.Modem.IRZ.Shell;
using IBox.Modem.IRZ.Protocol;
using IBox.Modem.IRZ.Core;

namespace IBoxUsbModemUnitTest
{
    public class MU709IRZModemProtocolUnit
    {
        private readonly Logger _logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();

        private ModemStatus _status = new ModemStatus
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

        [Fact(DisplayName = "Get echo handle test")]
        public void EchoHandlerTest()
        {
            var request = new ModemRequestContext
            {
                Response = _status,
                Connection = _configuration
            };
            var handlerRequest = new RequestEchoHandler();
            var result = handlerRequest.Handel(request, "AT");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Get atz handle test")]
        public void ResetHandlerTest()
        {
            var request = new ModemRequestContext
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new RequestResetHandler();
            var result = handler.Handel(request, "ATZ");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Get manufacturer test")]
        public void RequestManufacturerTest()
        {
            var request = new ModemRequestContext
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new RequestManufaturerHandler();
            var result = handler.Handel(request, "AT+GMI");
            result.Should().NotBeNull();
            result.Response.Manufacturer.Should().NotBeEmpty();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Get model name handle test")]
        public void RequestModelHandlerTest()
        {
            var request = new ModemRequestContext
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new RequestModelHandler();
            var result = handler.Handel(request, "AT+GMM");
            result.Should().NotBeNull();
            result.Response.ModelName.Should().NotBeEmpty();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Get IMSI handle test")]
        public void ImsiHandlerTest()
        {
            var request = new ModemRequestContext
            {
                Response = _status,
                Connection = _configuration
            };
            var handler = new RequestImsiHandler();
            var result = handler.Handel(request, "AT+CIMI");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }
    }
}
