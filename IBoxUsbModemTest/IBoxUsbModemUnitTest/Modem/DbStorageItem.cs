using Newtonsoft.Json;

namespace IBoxUsbModemUnitTest.Modem
{
    public class DbStorageItem // : клон IBox.Domain.Model.StorageItem
    {
        /// <summary> Первичный ключ SQLite. !!! Вручную не устанавливается !!! </summary>
        [JsonProperty("id")]
        //[AutoIncrement, PrimaryKey, NotNull]
        public int Id { get; set; }
    }
}
