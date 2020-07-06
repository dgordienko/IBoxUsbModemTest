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


        [Fact(DisplayName = "interface IModem, method GetModemStatus()")]
        public void GetModemStatusTest()
        {
            // modem commands
            var echo = new RequestEchoHandler();
            var imai = new RequestImeiHandler();
            var imsi = new RequestImsiHandler();
            var manufacturer = new RequestManufaturerHandler();
            var model = new RequestModelHandler();
            var atz = new RequestResetHandler();
            var revision = new RequestRevisionIdentificationHandler();
            var quality = new RequestSignalQualityHandler();

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


           echo.SetNext(atz)
            .SetNext(model)
            .SetNext(manufacturer)
            .SetNext(imsi)
            .SetNext(imai)
            .SetNext(revision)
            .SetNext(quality);  
                         
            var commads = new List<string> 
            { 
                "AT","AT+GMI"
            };
            ModemCommandWorkflow.Commands = commads;
            var result = ModemCommandWorkflow.Workflow(request, echo);

        }
    }

    public static class ModemCommandWorkflow
    {
        public static List<string> Commands { get; set; }
        public static List<ModemRequestContext> Workflow(ModemRequestContext context, 
            AbstractModemCommandHandler handler)
        {
            var request = context;
            List<ModemRequestContext> results = new List<ModemRequestContext>();
            foreach (var command in Commands)
            {
               var result =  handler.Handle(request,command);
               results.Add(result);
            }
            return results;
        }
    }
}