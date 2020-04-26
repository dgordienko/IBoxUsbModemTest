namespace IBoxUsbModemUnitTest.Modem
{
    public class SignalQuality
    {
        /// <summary>
        /// %
        /// </summary>
        public string Percent { get; set; }

        /// <summary>
        /// http://en.wikipedia.org/wiki/DBm - (sometimes dBmW) power referenced to one milliwatt (mW).
        /// </summary>
        public int dBmW { get; set; }

        public override string ToString()
        {
            if (!IsValid)
                return "Undefined";
            return string.Format("{0}dBmW {1}%", dBmW, Percent);
        }

        public bool IsValid { get; set; }
    }
}
