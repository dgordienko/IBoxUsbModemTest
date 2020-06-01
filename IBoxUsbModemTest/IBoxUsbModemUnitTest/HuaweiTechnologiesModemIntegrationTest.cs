using FluentAssertions;
using IBoxUsbModemUnitTest.Modem;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    /// <summary>
    /// Общий интеграционный тест получения статуса модема
    /// </summary>
    public class HuaweiTechnologiesModemIntegrationTest
    {
        private readonly Logger _logger = new LoggerConfiguration()
            .WriteTo.File("HuaweiTechnologiesModemIntegrationTest.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.Console().CreateLogger();


        private readonly ConnectConfiguration _configuration = new ConnectConfiguration
        {
            ConnectionType = ConnectionType.Usb,
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
            PortName = ConfigurationManager.AppSettings["port"]
        };

        [Fact(DisplayName = "Request modem status")]
        public void IntegrationModule()
        {
            var modem = new Modem.Modem(_logger, _configuration);
            //modem.ApplyConfiguration(_configuration);
            modem.Should().NotBeNull();
            Exception exception = null;
            ModemStatus status = null;
            try
            {
                status = modem.GetModemStatus();
            }
            catch (Exception e)
            {
                exception = e;
            }
            exception.Should().BeNull();
            status.Should().NotBeNull();

            status.IsSuccess.Should().BeTrue();
            status.OperatorName.Should().NotBeNull();
            status.SerialNumber.Should().NotBeNull();
        }

        /// <summary>
        /// Перебор списка с именами портов и отправыка команды echo
        /// </summary>
        [Fact(DisplayName ="Found active port")]
        public void PortConnectionCheck()
        {

            var portList = new List<string>
            {
                "ttyUSB0",
                "ttyUSB1",
                "ttyUSB2",
                "ttyUSB3",
                "ttyUSB4",
                "ttyUSB5",
                "ttyUSB6",
                "ttyUSB7",
                "ttyUSB8",
                "ttyUSB9"
            };
            var errorCount = 0;
            foreach (var item in portList)
            {
                var configuration = new ConnectConfiguration
                {
                    ConnectionType = ConnectionType.Usb,
                    BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
                    PortName = $"/dev/{item}"
                };
                try
                {
                    var modem = new Modem.Modem(_logger, configuration);
                    modem.ApplyConfiguration(configuration);
                    var port = modem.ConfigurePort();
                    var response = modem.SendATCommand(port, "ATE");
                    _logger.Debug(response);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.Message, ex.StackTrace);
                    errorCount++;
                    //continue;
                }                                
            }
            errorCount.Should().BeLessThan(portList.Count);
        }
    }
}
