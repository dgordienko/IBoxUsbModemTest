using FluentAssertions;
using IBoxUsbModemUnitTest.Modem;
using Serilog;
using Serilog.Core;
using System;
using System.Configuration;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    /// <summary>
    /// Общий интеграционный тест получения статуса модема
    /// </summary>
    public class HuaweiTechnologiesModemIntegrationTest
    {
        private readonly Logger _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

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
    }
}
