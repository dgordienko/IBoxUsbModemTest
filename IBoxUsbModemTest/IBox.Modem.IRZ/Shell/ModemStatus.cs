using Newtonsoft.Json;

namespace IBox.Modem.IRZ.Shell
{
    public sealed class ModemStatus
    {
        public bool IsSuccess { get; set; }
        public string State { get; set; }

        public string Manufacturer { get; set; }
        public string ModelName { get; set; }
        public ModelModem AsModel()
        {
            var res = ModelModemEx.ParseManufacturer(Manufacturer);
            if (res == ModelModem.Unknown)
                res = ModelModemEx.ParseModelName(ModelName);
            return res;
        }
        public string GetFullModelName()
        {
            if (string.IsNullOrEmpty(ModelName))
                return Manufacturer;
            if (string.IsNullOrEmpty(Manufacturer))
                return ModelName;
            return Manufacturer + " " + ModelName;
        }

        [JsonProperty("revisionId")]
        public string RevisionId { get; set; }

        [JsonProperty("serialnumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("networkQuality")]
        public SignalQuality SignalQuality { get; set; }

        [JsonProperty("imsi")]
        public string Imsi { get; set; }
        [JsonProperty("imei")]
        public string Imei { get; set; }

        public string OperatorName { get; set; }
        public GprsProvider AsGprsProvider()
        {
            return GprsProviderEx.ParseSimIMSI(Imsi);
            /*
            var res = GprsProviderEx.ParseSimIMSI(Imsi);
            if (res == GprsProvider.Undefined)
                res = GprsProviderEx.ParseSimRegisteredOperator(OperatorName);
            return res;
            */
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var oth = obj as ModemStatus;
            if (oth == null)
                return false;
            return (IsSuccess == oth.IsSuccess) &&
                State.Equals(oth.State) &&
                Manufacturer.Equals(oth.Manufacturer) &&
                ModelName.Equals(oth.ModelName) &&
                SerialNumber.Equals(oth.SerialNumber) &&
                (SignalQuality == oth.SignalQuality) &&
                Imsi.Equals(oth.Imsi) &&
                Imei.Equals(oth.Imei) &&
                OperatorName.Equals(oth.OperatorName);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^
                IsSuccess.GetHashCode() ^
                State.GetHashCode() ^
                Manufacturer.GetHashCode() ^
                ModelName.GetHashCode() ^
                SerialNumber.GetHashCode() ^
                SignalQuality.GetHashCode() ^
                Imsi.GetHashCode() ^
                Imei.GetHashCode() ^
                OperatorName.GetHashCode();
        }

        public override string ToString()
        {
            return $"succ={IsSuccess}, state={State}, operator={OperatorName}, model='{Manufacturer} {ModelName}', " +
                   $"sn={SerialNumber}, signal=[{SignalQuality}], imsi={Imsi}, imei={Imei}";
        }

        public ModemStatus Clone()
        {
            return new ModemStatus
            {
                IsSuccess = IsSuccess,
                State = State,
                Manufacturer = Manufacturer,
                ModelName = ModelName,
                SerialNumber = SerialNumber,
                SignalQuality = SignalQuality,
                Imsi = Imsi,
                Imei = Imei,
                OperatorName = OperatorName
            };
        }
    }
}
