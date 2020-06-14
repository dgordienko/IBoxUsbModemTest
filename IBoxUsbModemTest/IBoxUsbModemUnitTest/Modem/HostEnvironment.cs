using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;

namespace IBoxUsbModemUnitTest.Modem
{
    public static class HostEnvironment
    {
        private static readonly HostOsType HostOsType;

        public static string OsVersion;
        //DEVSPACE-1752
        //public static readonly LinuxIBoxOsVersion IBoxOsVersion;
        public static HostOsType HostOs { get { return HostOsType; } }
        public static bool IsLinux { get { return HostOsType == HostOsType.Linux; } }
        public static bool IsWindows { get { return HostOsType == HostOsType.Windows; } }

        public static CultureInfo LocaleInfo { get; private set; }

        /*
         //DEVSPACE-1752 - no need
        public enum LinuxIBoxOsVersion
        {
            Unknown,
          //Hollow, // iboxOs 1.3.8 - kernel 3.14.6-1
            Vegas,  // iboxOs 1.4.2 - kernel 3.14.6-1
            Apollon // iboxOs 1.5.0 - kernel 3.18.6-1
        }
         */

        static HostEnvironment()
        {
            var platformNumber = (int)Environment.OSVersion.Platform;

            //determine if the host is a *nix operating system these numbers take into account historic values, do an internet search for details
            if ((platformNumber == 4) || (platformNumber == 6) || (platformNumber == 128))
                // running on *nix
                HostOsType = HostOsType.Linux;
            else
                // not running on *nix (this function defaults to windows)
                HostOsType = HostOsType.Windows;

            OsVersion = Environment.OSVersion.ToString();
            //DEVSPACE-1752 - no need to output this in log - doubling!
            /*            if (IsLinux)
                        {
                            // ... see main

                            var ver = Environment.OSVersion.Version;
                            if (ver == new Version(3, 18, 6, 1))
                                IBoxOsVersion = LinuxIBoxOsVersion.Apollon;
                            else
                            if (ver == new Version(3, 14, 6, 1))
                                IBoxOsVersion = LinuxIBoxOsVersion.Vegas;

                            ShellConsole.WriteInfo("IBoxOsVersion = {0}     kernel=[{1}.{2}.{3}.{4}]", Environment.OSVersion, ver.Major, ver.Minor, ver.Build, ver.Revision);
                        }
                        else
                    */
            try
            {
                var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                // @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion"
                if (regKey != null)
                    OsVersion = $"{regKey.GetValue("ProductName")} ({OsVersion})";
            }
            catch (Exception ex)
            {
                ShellConsole.WriteError("Read WinOS version from registry: {0}", ex.Message);
                Debug.Assert(false, "Глянь... " + ex.Message);
            }

            LocaleInfo = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            if (LocaleInfo != null)
                LocaleInfo.NumberFormat.NumberDecimalSeparator = ".";
        }

        private static IEnumerable<string> GetAllSerialPorts()
        {
            return SerialPort.GetPortNames().Where(com => !com.EndsWith("USB0"));
        }

        private static readonly string[] AllSerialPorts = GetAllSerialPorts().ToArray();

        public static string CheckSerialPortName(string portName)
        {
            if (!GetSerialPorts().Contains(portName))
                throw new ArgumentException("Invalid port name " + portName);
            return portName;
        }
        public static string[] GetSerialPorts()
        {
            return AllSerialPorts;
        }
        public static string GetAvailableSerialPort(uint index)
        {
            if (AllSerialPorts.Any() && (AllSerialPorts.Length > index))
                return AllSerialPorts[index];
            return IsLinux ? "/dev/ttyS0" : "COM1";
        }

        private static readonly char[] NewLine = Environment.NewLine.ToCharArray();
        public static char[] NewLineAsCharArray { get { return NewLine; } }

        /// <summary> каталог откуда запущено приложение </summary>
        public static string AppPath()
        {
            // eq WinForms Application.ExecutablePath
            var location = Assembly.GetExecutingAssembly().Location;
            location = Path.GetDirectoryName(location); // new FileInfo(location).DirectoryName;
            return location;
        }

        /// <summary> расположение 'repository' - на 1 каталог выше запущенной прикладухи </summary>
        public static string RepositoryDir()
        {
            if (IsLinux)
                return "/home/ibox/iboxsoft/repository";

            var location = AppPath();

            var parent = Directory.GetParent(location);
            if (parent == null)
                return location;
            location = parent.FullName;

            return location;
        }

        /// <summary> на 2 каталога выше запущенной прикладухи (для каталогов updates sqlite keystore windows_settings) </summary>
        public static string AppPath2Up()
        {
            var location = AppPath();

            var parent = Directory.GetParent(location);
            if (parent == null)
                return location;
            location = parent.FullName;

            parent = Directory.GetParent(location);
            if (parent == null)
                return location;
            location = parent.FullName;

            return location;
        }

        public static Func<TimeSpan> FuncUpTime;
        public static TimeSpan UpTime
        {
            get
            {
                try
                {
                    if (FuncUpTime != null)
                        return FuncUpTime();
                    //using (var uptime = new PerformanceCounter("System", "System Up Time"))
                    //{
                    //    uptime.NextValue(); // Call this an extra time before reading its value
                    //    return TimeSpan.FromSeconds(uptime.NextValue());
                    //}
                }
                catch (Exception ex)
                {
                    var err = "HostEnvironment::UpTime: call FuncUpTime: " + ex.Message;
                    ShellConsole.WriteError(err);
                    Debug.Assert(false, err);
                }
                return TimeSpan.FromMilliseconds(Environment.TickCount);
            }
        }

        public static Func<DateTime> FuncUpTimeAsDateTime;
        public static DateTime UpTimeAsDateTime
        {
            get
            {
                try
                {
                    if (FuncUpTimeAsDateTime != null)
                        return FuncUpTimeAsDateTime();
                }
                catch (Exception ex)
                {
                    var err = "HostEnvironment::UpTimeAsDateTime: call FuncUpTimeAsDateTime: " + ex.Message;
                    ShellConsole.WriteError(err);
                    Debug.Assert(false, err);
                }
                return DateTime.Now.Subtract(UpTime);
            }
        }

        public static string ExeAssemblyVersion => FileVersionInfo
            .GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}
