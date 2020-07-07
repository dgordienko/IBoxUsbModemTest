using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    /// Команда Echo (AT)
    /// </summary>
    public sealed class RequestEchoHandler : AbstractModemCommandHandler
    {
        private const string NOECHO = @"Нет ответа ЭХО";
        private const string ECHO = @"ЭХО";
        private const string AT = @"AT";
        private const string OK = @"OK";

        protected override string ModemCommand => AT;

        protected override void OnDataReceived(object sender, string response)
        {
            modemStatus.IsSuccess = Regex.Matches(response, $"{OK}", RegexOptions.IgnoreCase).Count > 0;
            received = false;
        }
    }
}