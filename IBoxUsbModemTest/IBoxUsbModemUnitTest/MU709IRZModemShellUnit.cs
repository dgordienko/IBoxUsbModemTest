using FluentAssertions;
using IBox.Modem.IRZ.Shell;
using Serilog;
using Serilog.Core;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    /// <summary>
    /// Test include IRZ in Shell
    /// </summary>
    public class MU709IRZModemShellUnit
    {
        private readonly Logger _logger = new LoggerConfiguration()
            .WriteTo.Console().CreateLogger();
            

        [Theory(DisplayName ="Parser model name")]
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

    }
}
