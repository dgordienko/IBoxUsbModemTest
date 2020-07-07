using System.Configuration;
using FluentAssertions;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Protocol;
using IBox.Modem.IRZ.Shell;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    public class Mu709IrzModemProtocolUnit
    {

        private readonly ConnectConfiguration _configuration = new ConnectConfiguration
        {
            ConnectionType = ConnectionType.Usb,
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
            PortName = ConfigurationManager.AppSettings["port"]
        };

        [Fact(DisplayName = "Request echo handler test")]
        public void EchoHandleTest()
        {
            var status = new ModemStatus
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
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var handlerRequest = new RequestEchoHandler();
            var result = handlerRequest.Handle(request, "AT");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request IMSI handler test",Skip ="insert sim card")]
        public void ImsiHandleTest()
        {
            var status = new ModemStatus
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
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var handler = new RequestImsiHandler();
            var result = handler.Handle(request, "AT+CIMI");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request manufacturer test")]
        public void RequestManufacturerTest()
        {
            var status = new ModemStatus
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
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var handler = new RequestManufaturerHandler();
            var result = handler.Handle(request, "AT+GMI");
            result.Should().NotBeNull();
            result.Response.Manufacturer.Should().NotBeEmpty();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request model name handler test")]
        public void RequestModelTest()
        {
            var status = new ModemStatus
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
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var handler = new RequestModelHandler();
            var result = handler.Handle(request, "AT+GMM");
            result.Should().NotBeNull();
            result.Response.ModelName.Should().NotBeEmpty();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request atz handler test")]
        public void ResetHandlerTest()
        {
            var status = new ModemStatus
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
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var handler = new RequestResetHandler();
            var result = handler.Handle(request, "ATZ");
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request revision identification of software status")]
        public void RequestRevisionIdentificationHandleTest()
        {
            var status = new ModemStatus
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
            var command = "AT+CGMR";
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var handle = new RequestRevisionIdentificationHandler();
            var result = handle.Handle(request, command);
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request network status", Skip = "insert sim card")]
        public void RequestNetworkStatusHandelTest()
        {
            var status = new ModemStatus
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
            var handle = new RequestNetworkStatusHandler();
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var command = @"AT+CREG?";
            var result = handle.Handle(request, command);
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Request signal quality", Skip = "insert sim card")]
        public void RequestSignalQualityHandlerTest()
        {
            var status = new ModemStatus
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
            var handle = new RequestSignalQualityHandler();
            var request = new ModemRequestContext
            {
                Response = status,
                Connection = _configuration
            };
            var command = @"AT+CSQ";
            var result = handle.Handle(request, command);
            result.Should().NotBeNull();
            result.Response.IsSuccess.Should().BeTrue();
        }
    }
}