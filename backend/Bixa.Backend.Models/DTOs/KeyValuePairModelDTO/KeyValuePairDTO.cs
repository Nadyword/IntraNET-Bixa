namespace Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;

public class KeyValuePairDTO<TKey, TValue>
{
    public TKey? Key { get; set; }
    public TValue? Value { get; set; }
}