using System;
using System.Text.RegularExpressions;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    public sealed class RequestModelHandler : AbstractModemCommandHandler
    {
        private const string Command = "AT+GMM";
        private const string OK = "OK";

        protected override string ModemCommand => Command;

        protected override void OnDataReceived(object sender, string response)
        {
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            if (matches.Count >= 2 && OK.Equals(matches[matches.Count - 1].Value,
                StringComparison.OrdinalIgnoreCase))
            {
                modemStatus.IsSuccess = true;
                modemStatus.ModelName = matches[matches.Count - 2].Value.Trim();
            }
            received = false;
            (sender as ModemManager)?.Close();
        }
    }
}