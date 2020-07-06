namespace IBox.Modem.IRZ.Shell
{
    public class SignalQuality
    {
        /// <summary>
        ///     %
        /// </summary>
        public string Percent { get; set; }

        /// <summary>
        ///     http://en.wikipedia.org/wiki/DBm - (sometimes dBmW) power referenced to one milliwatt (mW).
        /// </summary>
        public int dBmW { get; set; }

        public bool IsValid { get; set; }

        public override string ToString()
        {
            return !IsValid ? "Undefined" : $"{dBmW}dBmW {Percent}%";
        }
    }
}