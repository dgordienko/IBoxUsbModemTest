using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using IBox.Modem.IRZ.Core;
using IBox.Modem.IRZ.Shell;

namespace IBox.Modem.IRZ.Protocol
{
    /// <summary>
    /// SignalQuality "AT+CSQ"
    /// </summary>
    public sealed class RequestSignalQualityHandler : AbstractModemCommandHandler
    {
        private const string Command = "AT+CSQ";
        private const int Sleep = 500;

        protected override string ModemCommand => Command;

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public override ModemRequestContext Handle(ModemRequestContext request, string param)
        {
            var signalQuality = new SignalQuality
            {
                dBmW = 0,
                IsValid = false,
                Percent = string.Empty
            };
            if (param == Command)
            {
                var result = request.Response;
                var helper = ModemManager.Instance;
                var received = false;
                helper.OnSerialPortOpened += (sender, response) =>
                {
                    if (!response) return;
                    try
                    {
                        received = true;
                        helper.SendString(param);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                        request.Response.IsSuccess = false;
                        //request.Description.Add($"Error send {Command} - {exception.Message}");
                        received = false;
#if DEBUG
                        throw;
#endif
                    }
                };
                helper.OnStatusChanged += (sender, response) => { Debug.WriteLine(response); };
                helper.OnDataReceived += (sender, response) =>
                {
                    if (string.IsNullOrEmpty(response))
                    {
                        result.SignalQuality = signalQuality;
                    }
                    else
                    {
                        try
                        {
                            var matches = Regex.Matches(response, @"[\S ]+", RegexOptions.Singleline);
                            if (matches.Count >= 2 && "OK".Equals(matches[matches.Count - 1].Value,
                                StringComparison.OrdinalIgnoreCase))
                            {
                                matches = Regex.Matches(matches[matches.Count - 2].Value, @"\d+", RegexOptions.None);
                                if (matches.Count > 0)
                                {
                                    var r = int.Parse(matches[0].Value);
                                    if (r > 32)
                                    {
                                        signalQuality.dBmW = 0;
                                        result.SignalQuality.Percent = "0.0";
                                        result.SignalQuality.IsValid = false;
                                    }

                                    if (r == 99)
                                    {
                                        result.SignalQuality = signalQuality;
                                    }

                                    else
                                    {
                                        result.SignalQuality.dBmW = -113 + (r << 1);
                                        result.SignalQuality.Percent = (r * 100.0 / 31.0).ToString("F2");
                                        result.SignalQuality.IsValid = true;
                                        result.IsSuccess = true;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            received = false;
                            (sender as ModemManager)?.Close();
                        }
                    }
                };
                if (!helper.IsOpen)
                {
                    helper.Open(request.Connection.PortName);
                }
                else
                {
                    helper.Close();
                    helper.Open(request.Connection.PortName);
                }
                lock (_locker)
                {
                    while (received)
                    {
                        Monitor.Wait(_locker, TimeSpan.FromMilliseconds(Sleep));
                    }
                }

                return request;
            }
            return base.Handle(request, param);
        }

        protected override void OnDataReceived(object sender, string response)
        {
            throw new NotImplementedException();
        }

        protected override void OnStatusChanged(object sender, string response)
        {
            throw new NotImplementedException();
        }
    }
}