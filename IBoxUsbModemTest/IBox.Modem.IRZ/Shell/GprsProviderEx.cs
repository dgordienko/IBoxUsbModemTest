using System;
using System.IO;

namespace IBox.Modem.IRZ.Shell
{
    public static class GprsProviderEx
    {
        private const string LinuxBridgePath = "/etc/ppp/peers/";

        public static string ProviderName(this GprsProvider self)
        {
            switch (self)
            {
                case GprsProvider.Undefined: return "Unknown";
                case GprsProvider.Mts: return "MTS";
                case GprsProvider.KyivStar: return "KyivStar";
                case GprsProvider.Cdma: return "CDMA";
                case GprsProvider.Life: return "Life";
                //case GprsProvider.Ukrtelecom: return "Ukrtelecom";
            }

            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        private static string LinuxName(this GprsProvider self)
        {
            return ProviderName(self).Trim().Replace(' ', '_').ToLower();
        }

        public static string LinuxScriptFile(this GprsProvider self, bool fullPath)
        {
            if (fullPath)
                return "/etc/chatscript/" + self.LinuxName() + "-connect";
            return self.LinuxName() + "-connect";
        }

        public static string LinuxBridgeFile(this GprsProvider self, bool fullPath)
        {
            if (fullPath)
                return LinuxBridgePath + self.LinuxName();
            return self.LinuxName();
        }

        public static string LinuxLogFile(this GprsProvider self)
        {
            return "/var/log/" + self.LinuxName();
        }

        public static GprsProvider ParseSimRegisteredOperator(string val)
        {
            // http://da3rx.com/K750toW800/FS/tpa/preset/default/networklist
            // http://docs.openmoko.org/trac/attachment/ticket/666/gsmlog.txt
            // http://sandbox.dfrws.org/2010/jacob/evidence/part_tpa/part1/Fat%20File%20System/tpa/preset/default/networklist
            if ("25501".Equals(val) || "UA UMC".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "UMC".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "MTS UKR".Equals(val, StringComparison.InvariantCultureIgnoreCase))
                return GprsProvider.Mts;
            if ("25503".Equals(val) || "UA-KYIVSTAR".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "Kyivstar".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "UA-KS".Equals(val, StringComparison.InvariantCultureIgnoreCase))
                return GprsProvider.KyivStar;
            if ("25506".Equals(val) || "UA ASTELIT".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "CC 255 NC 06".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "UA life:)".Equals(val, StringComparison.InvariantCultureIgnoreCase) ||
                "life:)".Equals(val, StringComparison.InvariantCultureIgnoreCase))
                return GprsProvider.Life;
            //if ("25505".Equals(val) || "UA-GT".Equals(val, StringComparison.InvariantCultureIgnoreCase))
            //    return GprsProvider.GoldenTelecom;
            return GprsProvider.Undefined;
        }

        public static GprsProvider ParseSimIMSI(string val)
        {
            if (val.StartsWith("25501"))
                return GprsProvider.Mts;
            if (val.StartsWith("25503"))
                return GprsProvider.KyivStar;
            if (val.StartsWith("25506"))
                return GprsProvider.Life;
            return GprsProvider.Undefined;
        }

        public static GprsProvider Parse(string val)
        {
            foreach (GprsProvider e in Enum.GetValues(typeof(GprsProvider)))
                if (e.ProviderName().Equals(val, StringComparison.OrdinalIgnoreCase) ||
                    e.ToString().Equals(val, StringComparison.OrdinalIgnoreCase))
                    return e;
            throw new ArgumentException(string.Format("Unknown value '{0}'", val));
        }

        public static GprsProvider NextKnown(this GprsProvider self)
        {
            switch (self)
            {
                case GprsProvider.Undefined: return GprsProvider.Mts;
                case GprsProvider.Mts: return GprsProvider.KyivStar;
                case GprsProvider.KyivStar: return GprsProvider.Life;
                case GprsProvider.Life: return GprsProvider.Mts; // Cdma;
                case GprsProvider.Cdma: return GprsProvider.Mts;
            }

            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        public static string CallNumber(this GprsProvider self, int tryIndex)
        {
            switch (self)
            {
                case GprsProvider.Undefined: return string.Empty;
                case GprsProvider.Mts: return "*99#";
                case GprsProvider.KyivStar: return (tryIndex & 1) == 0 ? "*99***1#" : "*99#";
                case GprsProvider.Life: return (tryIndex & 1) == 0 ? "*99***1#" : "*99#";
                case GprsProvider.Cdma: return "#777";
            }

            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        public static string AdditionalInitCommand(this GprsProvider self)
        {
            const string cnstInit = "AT+CGDCONT=1,\"IP\",\"ibox.";
            switch (self)
            {
                case GprsProvider.Undefined: return string.Empty;
                case GprsProvider.Mts: return cnstInit + "umc.ua\""; //"AT+CGDCONT=1,\"IP\",\"ibox.umc.ua\"";
                case GprsProvider.KyivStar:
                    return cnstInit + "kyivstar.net\""; //"AT +CGDCONT=1,ip,\"ibox.kyivstar.net\"";
                case GprsProvider.Life:
                    return
                        cnstInit +
                        "life.ua\""; //"AT +CGDCONT=1,ip,\"ibox.life.ua\""; // "AT+CGDCONT=1,ip,\"internet\""; // "AT+CGDCONT=1,ip,\"www.djuice.com.ua\"";
                case GprsProvider.Cdma: return "AT+CRM=1";
            }

            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        public static string AdditionalInitCommand(this GprsProvider self, ModelModem eModelModem)
        {
            switch (eModelModem)
            {
                //case ModelModem.None:
                case ModelModem.Unknown:
                case ModelModem.SiemensMC35:
                case ModelModem.Cinterion: //DEVSPACE-3943
                case ModelModem.PantechUM175:
                    return self.AdditionalInitCommand();
                case ModelModem.Wavecom: return string.Empty;
                case ModelModem.MU709:
                    return self.AdditionalInitCommand(); // TODO: выяснить при тестировании
            }

            throw new ArgumentOutOfRangeException("...добавь case...");
        }

        // https://www.assembla.com/spaces/ibox-processing/wiki/Linux_I_GPRS-connection_I_configuration
        /// <summary>
        ///     Меняем настройку модема - в консоли  cp /etc/chatscript/life-connect /etc/chatscript/mts-connect
        ///     но пишем весь файл
        /// </summary>
        /// <param name="ModelModem"></param>
        public static void CreateScriptFile(this GprsProvider provider, ModelModem modelModem, int tryIndex)
        {
            var file = provider.LinuxScriptFile(true);
            {
                var filePreconfig = file + '-' + modelModem.ShortName();
                if (File.Exists(filePreconfig))
                {
                    File.Copy(filePreconfig, file, true);
                    return;
                }
            }
            using (var writer = new StreamWriter(file, false))
            {
                writer.WriteLine("TIMEOUT 45");
                writer.WriteLine("ABORT 'NO ANSWER'");
                writer.WriteLine("ABORT 'BUSY'");
                writer.WriteLine("ABORT 'NO CARRIER'");
                writer.WriteLine("SAY 'Starting " + provider.ProviderName() + " GPRS connection\\n'");
                writer.WriteLine("ABORT ERROR");
                writer.WriteLine("'' 'ATZ'");
                writer.WriteLine("OK 'ATE1'");
                var addCmd = provider.AdditionalInitCommand(modelModem);
                if (!string.IsNullOrEmpty(addCmd))
                    writer.WriteLine("OK '{0}'", addCmd);
                writer.WriteLine("OK 'ATD{0}'", provider.CallNumber(tryIndex));
                writer.WriteLine("CONNECT c");
            }
        }

        private static string ReadPreconfig(ModelModem modem)
        {
            var file = LinuxBridgePath + "_" + modem.Description().Replace(" ", "") + ".cfg";
            if (!File.Exists(file))
                using (var writer = new StreamWriter(file, false))
                {
                    writer.WriteLine("debug");
                    if (modem == ModelModem.SiemensMC35 || modem == ModelModem.Cinterion) //DEVSPACE-3943
                    {
                        writer.WriteLine("noauth"); // TODO !!! login psw
                        writer.WriteLine("defaultroute");
                        writer.WriteLine("usepeerdns"); //  get DNS from the provider
                        writer.WriteLine("persist");
                        writer.WriteLine("noipdefault");
                        writer.WriteLine("lock");
                        writer.WriteLine("modem");
                        writer.WriteLine("novjccomp");
                        writer.WriteLine("nopcomp");
                        writer.WriteLine("noaccomp");
                        writer.WriteLine("nodeflate");
                        writer.WriteLine("novj");
                    }
                    else if (modem == ModelModem.Wavecom)
                    {
                        // http://diff.org.ua/archives/212

                        writer.WriteLine("nobsdcomp");
                        writer.WriteLine("nodeflate");
                        //writer.WriteLine("#demand");
                        writer.WriteLine("noauth");
                        writer.WriteLine("noipdefault");
                        writer.WriteLine("usepeerdns");
                        writer.WriteLine("defaultroute");
                        //writer.WriteLine("#nodetach");
                        writer.WriteLine("persist");
                        writer.WriteLine("novj");
                        writer.WriteLine("crtscts");
                        writer.WriteLine("modem");
                        writer.WriteLine("lock");
                        writer.WriteLine("user ibox");
                    }
                }

            return File.ReadAllText(file);
        }

        /// <summary>
        ///     Настраиваем бридж cp /etc/ppp/peers/life /etc/ppp/peers/mts
        ///     Открываем для редактирования nano -w /etc/ppp/peers/mts
        ///     Меняем connect '/usr/sbin/chat -v -f /etc/chatscript/life-connect'   на connect '/usr/sbin/chat -v -f
        ///     /etc/chatscript/mts-connect'
        ///     ( в начале открывшегося файла должен быть указан порт, по которому подключен модем скорее всего dev/ttyS0 )
        ///     но пишем весь файл
        /// </summary>
        public static void CreateBridgeFile(this GprsProvider provider, ModelModem modelModem, string ttyDevice,
            int speed)
        {
            var file = provider.LinuxBridgeFile(true);
            {
                var filePreconfig = file + '-' + modelModem.ShortName();
                if (File.Exists(filePreconfig))
                {
                    File.Copy(filePreconfig, file, true);
                    ShellConsole.WriteInfo("Modem.CreateBridgeFile: Copy {0} to {1}", filePreconfig, file);
                    return;
                }
            }
            try
            {
                using (var writer = new StreamWriter(file, false))
                {
                    ShellConsole.WriteInfo("Modem.CreateBridgeFile: WriteFile {0}", file);

                    writer.Write(ttyDevice); // tty device 
                    writer.Write(" ");
                    writer.WriteLine(speed); // Serial port line speed
                    var preconfig = ReadPreconfig(modelModem);
                    writer.Write(preconfig);
                    writer.WriteLine("connect '/usr/sbin/chat -e -v -f " + provider.LinuxScriptFile(true) + "'");

                    if (modelModem == ModelModem.Wavecom && preconfig.Contains("user ibox"))
                        try
                        {
                            using (var writer2 = new StreamWriter("/etc/ppp/pap-secrets", false))
                            {
                                writer2.WriteLine("# Secrets for authentication using PAP");
                                writer2.WriteLine("# client server  secret          IP addresses");
                                writer2.WriteLine("ibox * 1 *");
                            }
                        }
                        catch (Exception ex)
                        {
                            ShellConsole.WriteInfo("Modem. CreateBridgeFile: WriterPapSecr: " + ex.Message);
                        }
                }
            }
            catch (Exception ex)
            {
                ShellConsole.WriteInfo("Modem. CreateBridgeFile: WriteFileError: " + ex.Message);
            }
        }
    }
}