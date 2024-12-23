using System.Collections.ObjectModel;
using System.Text.Json;

namespace PluginBase.Services.Options;

public class OptionProvider(string path)
{
    public Dictionary<string, object?> OptionValues { get; set; } = null!;
    public required ReadOnlyDictionary<string, OptionConfig> OptionConfigs { get; init; }
    public object? this[string key] => OptionValues[key];
    public T? Get<T>(string key) => this[key] is T value ? value : default;
    public void Set(string key, object value) => OptionValues[key] = value;

    public ReadOnlyDictionary<string, OptionConfig> GetConfig()
    {
        foreach (var option in OptionConfigs)
            option.Value.Update();

        return OptionConfigs;
    }

    public void Reset()
    {
        OptionValues = new Dictionary<string, object?>();
        foreach (var option in OptionConfigs)
            OptionValues[option.Key] = option.Value.DefaultValue;
    }

    public void Validate()
    {
        // TODO: Implement validation
    }

    public async Task Load()
    {
        try
        {
            using var file = new StreamReader(path);
            var options = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(file.BaseStream);
            OptionValues = options!;
            Validate();
        }
        catch (Exception e)
        {
            // ignored
        }
        finally
        {
            if (OptionValues is null)
            {
                Reset();
                await Save();
            }
        }
    }

    public async Task Save()
    {
        try
        {
            Validate();
            await using var file = new StreamWriter(path);
            await JsonSerializer.SerializeAsync(file.BaseStream, OptionValues);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}