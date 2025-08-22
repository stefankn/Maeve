namespace Maeve.Database.KeyValueStore;

public interface IKeyValueStore {
    
    // - Functions

    public Task<string?> GetStringAsync(string key);
    public string? GetString(string key);
    public Task SetStringAsync(string? value, string key);
    public void SetString(string? value, string key);
    public Task<bool> GetBoolAsync(string key);
    public bool GetBool(string key);
    public Task SetBoolAsync(bool? value, string key);
    public void SetBool(bool? value, string key);
    public int? GetInt(string key);
    public void SetInt(int? value, string key);
    public TEnum? GetEnum<TEnum>(string key) where TEnum : struct, IConvertible;
    public void SetEnum<TEnum>(TEnum? value, string key) where TEnum : struct, IConvertible;
}