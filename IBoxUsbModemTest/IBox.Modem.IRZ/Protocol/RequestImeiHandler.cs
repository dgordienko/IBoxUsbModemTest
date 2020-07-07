using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    ///     request IMEI (Imei is serial number AT+CGSN)
    /// </summary>
    public sealed class RequestImeiHandler : AbstractModemCommandHandler
    {
        private const string Command = "AT+CGSN";
        protected override string ModemCommand => Command;

        protected override void OnDataReceived(object sender, string response)
        {
            var matches = Regex.Matches(response, @"\d{10,}", RegexOptions.IgnoreCase);
            modemStatus.Imei = modemStatus.SerialNumber = matches[0].Value;
            modemStatus.IsSuccess = modemStatus.SerialNumber == modemStatus.Imei;
            received = false;
            (sender as ModemManager)?.Close();
        }
    }
}