using System.Collections.Generic;
using System.Configuration;
using FluentAssertions;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Protocol;
using IBox.Modem.IRZ.Shell;
using Serilog;
using Serilog.Core;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    /// <summary>
    ///     Test include IRZ in Shell
    /// </summary>
    public class Mu709IrzModemShellUnit
    {
        private readonly Logger _logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();


        [Theory(DisplayName = "Parser model name")]
        [InlineData("MU709")]
        public void ModelNameExtention(string name)
        {
            var result = ModelModemEx.Parse(name);
            result.Should().NotBe(ModelModem.Unknown);
            result.Should().Be(ModelModem.MU709);
        }

        [Theory(DisplayName = "Parser manufacturer name")]
        [InlineData("Huawei Technologies Co., Ltd")]
        public void ManufacturerNameExtention(string value)
        {
            var result = ModelModemEx.ParseManufacturer(value);
            result.Should().NotBe(ModelModem.Unknown);
            result.Should().Be(ModelModem.MU709);
        }


        [Fact(DisplayName = "Get modem status")]
        public void GetModemStatusTest()
        {
            var configuration = new ConnectConfiguration
            {
                ConnectionType = ConnectionType.Usb,
                BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
                PortName = ConfigurationManager.AppSettings["port"]
            };

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
                Connection = configuration
            };

            var commands = new Dictionary<string, AbstractModemCommandHandler>
            {
                {"AT",new RequestEchoHandler() },
                {"AT+GMI", new RequestManufaturerHandler() },
                {"AT+GMM", new RequestModelHandler()},
                {"AT+CGMR", new RequestRevisionIdentificationHandler()}
            };
            var results = new Dictionary<string, ModemRequestContext>();
            foreach (var command in commands)
            {
                var commandString = command.Key;
                var commandClass = command.Value;
                var commandResult = commandClass.Handle(request, commandString);
                results.Add(commandString,commandResult);
            }

        }
    }


}