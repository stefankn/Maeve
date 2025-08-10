using Microsoft.EntityFrameworkCore;

namespace Maeve.Database.KeyValueStore;

public class KeyValueStore(IDbContextFactory<DataContext> dbContextFactory): IKeyValueStore {
    
    // - Functions

    public async Task<string?> GetStringAsync(string key) {
        return (await GetEntryAsync(key))?.Value;
    }

    public string? GetString(string key) {
        return GetEntry(key)?.Value;
    }
    
    public async Task SetStringAsync(string? value, string key) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();

        var entry = await dataContext.KeyValueEntries.SingleOrDefaultAsync(e => e.Key == key);
        if (entry != null) {
            if (value != null && entry.Value != value) {
                entry.Value = value;
                await dataContext.SaveChangesAsync();
            } else if (value == null) {
                dataContext.KeyValueEntries.Remove(entry);
                await dataContext.SaveChangesAsync();
            }
        } else {
            dataContext.KeyValueEntries.Add(new KeyValueEntry { Key = key, Value = value });
            await dataContext.SaveChangesAsync();
        }
    }

    public void SetString(string? value, string key) {
        using var dataContext = dbContextFactory.CreateDbContext();
        
        var entry = dataContext.KeyValueEntries.SingleOrDefault(e => e.Key == key);
        if (entry != null) {
            if (value != null && entry.Value != value) {
                entry.Value = value;
                dataContext.SaveChanges();
            } else if (value == null) {
                dataContext.KeyValueEntries.Remove(entry);
                dataContext.SaveChanges();
            }
        } else {
            dataContext.KeyValueEntries.Add(new KeyValueEntry { Key = key, Value = value });
            dataContext.SaveChanges();
        }
    }
    
    public async Task<bool> GetBoolAsync(string key) {
        return (await GetStringAsync(key))?.ToLower() == "true";
    }

    public bool GetBool(string key) {
        return GetString(key)?.ToLower() == "true";
    }

    public async Task SetBoolAsync(bool? value, string key) {
        await SetStringAsync(value?.ToString(), key);
    }

    public void SetBool(bool? value, string key) {
        SetString(value?.ToString(), key);
    }
    
    
    // - Private Functions

    private KeyValueEntry? GetEntry(string key) {
        using var dataContext = dbContextFactory.CreateDbContext();
        
        return dataContext.KeyValueEntries.SingleOrDefault(e => e.Key == key);
    }

    private async Task<KeyValueEntry?> GetEntryAsync(string key) {
        await using var dataContext = await dbContextFactory.CreateDbContextAsync();
        
        return await dataContext.KeyValueEntries.SingleOrDefaultAsync(e => e.Key == key);
    }
}