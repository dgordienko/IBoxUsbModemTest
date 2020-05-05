using FluentAssertions;
using IBoxUsbModemUnitTest.Modem;
using Serilog;
using Serilog.Core;
using System.Configuration;
using System.Text.RegularExpressions;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    /// <summary>
    /// Получение статуса сети 3G  модема
    /// </summary>
    public class HuaweiTechnologiesModemNetworkTest
    {
        private readonly Logger _logger;
        public HuaweiTechnologiesModemNetworkTest()
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


        /// <summary>
        /// Request IMSI
        /// </summary>
        /// <param name="command">AT+CIMI</param>
        [Theory(DisplayName = "Request IMSI")]
        [InlineData("AT+CIMI")]
        public void RequestIMSI(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var result = Regex.Matches(response, @"OK", RegexOptions.IgnoreCase).Count;

            result.Should().BeGreaterThan(0, "must be OK");
            var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
            matches.Should().NotBeEmpty("result must be defined");
            var IMSI = matches[0].Value;
            IMSI.Should().NotBeNullOrEmpty();

            var provider = GprsProviderEx.ParseSimIMSI(IMSI);
            var operatorName = GprsProviderEx.ProviderName(provider);
            _logger.Debug($"define {operatorName} as provider");
            provider.Should().NotBe(GprsProvider.Undefined, "provider must be defined");
        }

        /// <summary>
        /// Register network
        /// </summary>
        /// <param name="command">AT+CREG?</param>
        [Theory(DisplayName = "Register network")]
        [InlineData("AT+CREG?")]
        public void RegisterNetwork(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var description = "unknown";
            var answ = response.IndexOf("+CREG:");
            if (answ >= 0)
            {
                answ += "+CREG: 0,".Length;
                if (int.TryParse(response.Substring(answ, 1), out var status))
                    description = status.ToNetworkStatus();
            }
            description.Should().NotBe("unknown", "register network must be defined");
        }

        /// <summary>
        /// Request sigmal quality
        /// </summary>
        /// <param name="command" cref="AT+CSQ"/>
        [Theory(DisplayName = "Request signal quality")]
        [InlineData("AT+CSQ")]
        public void RequestSignalQuality(string command)
        {
            var modem = new Modem.Modem(_logger, _configuration);
            modem.ApplyConfiguration(_configuration);
            var usb = modem.ConfigurePort();
            var response = modem.SendATCommand(usb, command);
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            matches.Count.Should().BeGreaterThan(2);
            matches[matches.Count - 1].Value.ToLower().Should().Be("ok");
            var q = int.Parse(matches[0].Value);
            q.Should().BeLessThan(32);

            var quality = -113 + (q << 1);
            quality.Should().NotBe(null);
            var persent = (q * 100.0 / 31.0).ToString("F2");
            persent.Should().NotBe(null);
        }



    }
}
