using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     AT+CGMR  Request revision identification of software status
    /// </summary>
    public sealed class RequestRevisionIdentificationHandler : AbstractModemCommandHandler
    {
        private const string Command = "AT+CGMR";
        private const string Ok = "OK";

        protected override string ModemCommand => Command;

        protected override void OnDataReceived(object sender, string response)
        {
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            if (matches.Count >= 2 && Ok.Equals(matches[matches.Count - 1].Value,
                StringComparison.OrdinalIgnoreCase))
            {
                modemStatus.RevisionId = matches[matches.Count - 2].Value;
                modemStatus.IsSuccess = true;
            }
            received = false;
            (sender as ModemManager)?.Close();
        }
    }
}