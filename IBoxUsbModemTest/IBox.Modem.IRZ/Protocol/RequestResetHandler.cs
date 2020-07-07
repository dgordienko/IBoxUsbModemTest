using System.Text.RegularExpressions;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     Команда ATZ
    /// </summary>
    public sealed class RequestResetHandler : AbstractModemCommandHandler
    {
        private const string ATZ = "ATZ";
        private const string OK = "OK";
        private const string REDY = "Готов";
        private const string NOTREDY = "Не готов";
        private const int Sleep = 500;

        protected override string ModemCommand => ATZ;

        protected override void OnDataReceived(object sender, string response)
        {
            modemStatus.IsSuccess = Regex.Matches(response, $"{OK}",
                 RegexOptions.IgnoreCase).Count > 0;
            received = false;
            (sender as ModemManager)?.Close();
        }
    }
}