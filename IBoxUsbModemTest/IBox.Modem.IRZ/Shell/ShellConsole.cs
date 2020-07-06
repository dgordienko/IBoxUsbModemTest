using System;

namespace IBox.Modem.IRZ.Shell
{
    public static class ShellConsole
    {
        public const ConsoleColor InfoColor = ConsoleColor.Gray;
        public const ConsoleColor ErrorColor = ConsoleColor.Red;
        private static readonly ConsoleColor BackColor = Console.ForegroundColor;

        public static bool Colored;
        public static bool WithDateTime;

        static ShellConsole()
        {
            Colored = WithDateTime = HostEnvironment.IsWindows;
        }

        private static string DtNow => DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff]   ");

        #region WriteInfo

        public static void WriteInfo(object obj, ConsoleColor infoColor = InfoColor)
        {
            if (Colored) Console.ForegroundColor = InfoColor;
            if (WithDateTime)
                Console.WriteLine(DtNow + obj);
            else
                Console.WriteLine(obj);
            if (Colored) Console.ForegroundColor = BackColor;
        }

        public static void WriteInfo(string message, ConsoleColor infoColor = InfoColor)
        {
            if (Colored) Console.ForegroundColor = infoColor;
            if (WithDateTime)
                Console.WriteLine(DtNow + message);
            else
                Console.WriteLine(message);
            if (Colored) Console.ForegroundColor = BackColor;
        }

        public static void WriteInfo(string format, params object[] args)
        {
            if (Colored) Console.ForegroundColor = InfoColor;
            if (WithDateTime)
                Console.WriteLine(DtNow + string.Format(format, args));
            else
                Console.WriteLine(format, args);
            if (Colored) Console.ForegroundColor = BackColor;
        }

        #endregion


        #region WriteError

        public static void WriteError(object obj, ConsoleColor errorColor = ErrorColor)
        {
            if (Colored) Console.ForegroundColor = errorColor;
            if (WithDateTime)
                Console.WriteLine(DtNow + obj);
            else
                Console.Error.WriteLine(obj);
            if (Colored) Console.ForegroundColor = BackColor;
        }

        public static void WriteError(string message, ConsoleColor errorColor = ErrorColor)
        {
            if (Colored) Console.ForegroundColor = errorColor;
            if (WithDateTime)
                Console.WriteLine(DtNow + message);
            else
                Console.Error.WriteLine(message);
            if (Colored) Console.ForegroundColor = BackColor;
        }

        public static void WriteError(string format, params object[] args)
        {
            if (Colored) Console.ForegroundColor = ErrorColor;
            if (WithDateTime)
                Console.WriteLine(DtNow + string.Format(format, args));
            else
                Console.Error.WriteLine(format, args);
            if (Colored) Console.ForegroundColor = BackColor;
        }

        #endregion
    }
}