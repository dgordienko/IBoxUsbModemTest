using System.Diagnostics;
using Xunit;

namespace IBoxUsbModemUnitTest
{
    public class HuaweiTechnologiesModemInstallationTest
    {
        [Fact(DisplayName = "Set OS modem rules")]
        public void ApplyConfigurationRulles()
        {
           
        }


        static string ExecuteBashCommand(string command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thans to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }
    }


}
