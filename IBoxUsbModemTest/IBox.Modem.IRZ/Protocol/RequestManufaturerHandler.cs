using System;
using System.Text.RegularExpressions;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    public sealed class RequestManufaturerHandler : AbstractModemCommandHandler
    {
        private const string ATGMI = "AT+GMI";
        private const string DETECTED = "Detected";
        private const string UNKNOWN = "Unknown";
        private const string OK = "OK";

        protected override string ModemCommand => ATGMI;

        protected override void OnDataReceived(object sender, string response)
        {
            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
            if (matches.Count >= 2 &&
                OK.Equals(matches[matches.Count - 1].Value, StringComparison.OrdinalIgnoreCase))
            {
                modemStatus.Manufacturer = matches[matches.Count - 2].Value.Trim();
                var model = modemStatus.AsModel();
                modemStatus.ModelName = model.ShortName();
                modemStatus.IsSuccess = model != ModelModem.Unknown;
                var desc = model != ModelModem.Unknown ? DETECTED : UNKNOWN;
                //request.Description.Add($"{desc} {request.Response.ModelName}");
            }
            received = false;
            (sender as ModemManager)?.Close();
        }
    }
}