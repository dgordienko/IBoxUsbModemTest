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
    public class UsbModemTestCase
    {
        private readonly Logger _logger;
        public UsbModemTestCase()
        {            
            _logger = new LoggerConfiguration().
                WriteTo.Console().CreateLogger();
        }

        private readonly ConnectConfiguration _configuration = new ConnectConfiguration
        {
            ConnectionType = ConnectionType.Usb,
            BaudRate = int.Parse(ConfigurationManager.AppSettings["baudrate"]),
            PortName = ConfigurationManager.AppSettings["port"]
        };

        [Theory(DisplayName  = "ATE")]
        [InlineData("ATE")]
        public void Test_ATE(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            response.Should().NotBeNullOrEmpty("должна быть получена не пустая строка");
            var result = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count;
            result.Should().BeGreaterThan(0, "должен быть получен результат OK");
        }         

        [Theory(DisplayName ="АТ")]
        [InlineData("AT")]
        public void Test_AT(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var result = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count;
            result.Should().BeGreaterThan(0, "должен быть получен результат OK");
        }


        [Theory(DisplayName = "AT+CIMI")]
        [InlineData("AT+CIMI")]
        public void Test_AT_CIMI(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var result = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count;
            
            result.Should().BeGreaterThan(0, "должен быть получен результат OK");
            var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
            matches.Should().NotBeEmpty("результат должен быть получен");
            var IMSI = matches[0].Value;
            IMSI.Should().NotBeNullOrEmpty();

            var provider = GprsProviderEx.ParseSimIMSI(IMSI);
            var operatorName = GprsProviderEx.ProviderName(provider);
            _logger.Debug($"define {operatorName} as provider");
            provider.Should().NotBe(GprsProvider.Undefined,"provider должен быть определен");
        }

        [Theory(DisplayName = "Команда AT+CGMM")]
        [InlineData("AT+CGMM")]
        public void Test_AT_CGMM(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            
            var model = string.Empty;

            if ((matches.Count >= 2) &&"OK".Equals(matches[matches.Count - 1].Value,
                StringComparison.OrdinalIgnoreCase)){
                model = matches[matches.Count - 2].Value.Trim(); //MU709s-2
            }
            model.Should().NotBeNullOrEmpty("модель должна быть определена");
        }

        [Theory(DisplayName = "AT+GMI")]
        [InlineData("AT+GMI")]
        public void Test_AT_GMI(string command)
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
            manufacturer.Should().NotBeNullOrEmpty("производитель должен быть определен"); //Huawei Technologies Co., Ltd.
        }

        // AT+CREG?
        [Theory(DisplayName = "AT+CREG?")]
        [InlineData("AT+CREG?")]
        public void Test_AT_CREG(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var description = "неизвестно";
            var answ = response.IndexOf("+CREG:");
            if (answ >= 0)
            {
                answ += "+CREG: 0,".Length;
                if (int.TryParse(response.Substring(answ, 1), out var status))
                    description = status.ToNetworkStatus();
            }
            description.Should().NotBe("неизвестно","статус сети должен быть определен");
        }


        //AT+CGSN
        [Theory(DisplayName = "AT+CGSN")]
        [InlineData("AT+CGSN")]
        public void Test_AT_CGSN(string command)
        {
            //MU709s-2 AT^RESET

            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
            var imei = string.Empty;
            if (matches.Count > 0)
                imei = matches[0].Value;
            imei.Should().NotBeNullOrEmpty("imei должен быть определен");
        }

        //AT+CGMR
        [Theory(DisplayName = "AT+CGMR")]
        [InlineData("AT+CGMR")]
        public void Test_AT_CGMR(string command)
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

        //AT+CGMR
        [Theory(DisplayName = "AT^RESET")]
        [InlineData("AT^RESET")]
        public void Test_AT_RESET(string command)
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
    }
}
