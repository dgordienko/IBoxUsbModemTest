namespace IBox.Modem.IRZ.Shell
{
    public static class FormatResponse
    {
        public static string ToResponseData(this string value)
        {
            return $"{ value.Replace('\r', '|').Replace('\n', '|')}";
        }
    }
}
