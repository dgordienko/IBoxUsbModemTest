using FluentAssertions;
using IBoxUsbModemUnitTest.Modem;
using Serilog;
using Serilog.Core;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    /// <summary>
    /// Общие команды подключенного модема. <br/>
    /// Не требуется наличие активной сети 3G.
    /// </summary>
    public class HuaweiTechnologiesModemShellTest
    {
        private readonly Logger _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        private readonly ConnectConfiguration _configuration = new ConnectConfiguration
        {
            ConnectionType = ConnectionType.Usb,
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
            PortName = ConfigurationManager.AppSettings["port"]
        };


        [Theory(DisplayName ="Parser model name")]
        [InlineData("MU709s-2")]
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

        /// <summary>
        /// Reset the module
        /// </summary>
        /// <param name="command">AT^RESET</param>
        [Theory(DisplayName = "Reset the module",Skip ="reset nodem, not requarid")]        
        [InlineData("AT^RESET")]
        public void ResetModule(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            var revision = string.Empty;
            if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
                revision = matches[matches.Count - 2].Value;
            revision.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Echo command
        /// </summary>
        /// <param name="command">ATE</param>
        [Theory(DisplayName = "Echo command")]
        [InlineData("ATE")]
        public void TestEcho(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            response.Should().NotBeNullOrEmpty("response should not be null");
            var result = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count;
            result.Should().BeGreaterThan(0, "result must be OK");
        }

        [Theory(DisplayName = "Restore factory settings")]
        [InlineData("ATZ")]
        public void RestoreFactorySettings(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var result = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count;
            result.Should().BeGreaterThan(0, "result must be OK");
        }

        /// <summary>
        /// Product serial number request identification
        /// </summary>
        /// <param name="command">AT+CGSN</param>
        [Theory(DisplayName = "Request product serial number")]
        [InlineData("AT+CGSN")]
        [InlineData("AT+GSN")]
        public void RequestSerialNumber(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
            var imei = string.Empty;
            if (matches.Count > 0)
                imei = matches[0].Value;
            imei.Should().NotBeNullOrEmpty("serial number must be defined");
        }

        /// <summary>
        /// Версия ПO модема <br/>
        /// - 11.652.70.00.00
        /// </summary>
        /// <param name="command">AT+CGMR</param>
        [Theory(DisplayName = "Request software version")]
        [InlineData("AT+CGMR")]
        [InlineData("AT+GMR")]
        public void RequestSoftwareVersion(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            var revision = string.Empty;
            if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
                revision = matches[matches.Count - 2].Value;
            revision.Should().NotBeNullOrEmpty(); // 11.652.70.00.00
        }

        /// <summary>
        /// Определение производителя модема <br/
        /// - Huawei Technologies Co., Ltd.
        /// </summary>
        /// <param name="command">AT+GMI</param>
        [Theory(DisplayName = "Request manufacturer")]
        [InlineData("AT+GMI")]
        public void RequestManufacturer(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var manufacturer = string.Empty;
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
            {
                manufacturer = matches[matches.Count - 2].Value.Trim();
            }
            manufacturer.Should().NotBeNullOrEmpty("Manufacturer must be defined"); //Huawei Technologies Co., Ltd.
        }

        /// <summary>
        /// Определение модели модема <br/>
        ///  - MU709s-2
        /// </summary>
        /// <param name="command">Команда определения модели модема</param>
        [Theory(DisplayName = "Request model identication")]
        [InlineData("AT+CGMM")]
        public void RequestModelIdentifation(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);

            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);

            var model = string.Empty;

            if ((matches.Count >= 2) && "OK".Equals(matches[matches.Count - 1].Value,
                StringComparison.OrdinalIgnoreCase))
            {
                model = matches[matches.Count - 2].Value.Trim(); //
            }
            model.Should().NotBeNullOrEmpty("Model must be defined");
        }
    }
}
