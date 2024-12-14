using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using Task = Microsoft.Build.Utilities.Task;

namespace ReadJsonFileTask;

public class ReadPluginJsonFile : Task
{
    [Required]
    public required string JsonFilePath { get; set; }

    [Output]
    public string? Name { get; set; }

    [Output]
    public string? RegistryName { get; set; }

    [Output]
    public string? Description { get; set; }

    [Output]
    public string? Author { get; set; }

    [Output]
    public string? Version { get; set; }

    public override bool Execute()
    {
        try
        {
            var json = File.ReadAllText(JsonFilePath);
            var jsonObject = JObject.Parse(json);

            Name = jsonObject[nameof(Name)]?.ToString();
            RegistryName = jsonObject[nameof(RegistryName)]?.ToString();
            Description = jsonObject[nameof(Description)]?.ToString();
            Author = jsonObject[nameof(Author)]?.ToString();
            Version = jsonObject[nameof(Version)]?.ToString();

            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }
}