using System.Text.Json;

namespace SmartDevicePlugin;

public record SmartDeviceIndexEntry(string Ip, string Id, string Key);

public static class SmartDeviceIndex
{
    private static Lock s_Lock = new();
    private static List<SmartDeviceIndexEntry> s_Entries = new();
    private const string Path = "smart_device_index.json";

    public static SmartDeviceIndexEntry[] Entries
    {
        get
        {
            lock (s_Lock)
            {
                return s_Entries.ToArray();
            }
        }
    }

    public static void Load()
    {
        if (!File.Exists(Path))
        {
            return;
        }

        lock (s_Lock)
        {
            using var fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);
            s_Entries = JsonSerializer.Deserialize<List<SmartDeviceIndexEntry>>(sr.ReadToEnd())!;
        }
    }

    public static void AddEntry(string ip, string id, string key)
    {
        lock (s_Lock)
        {
            s_Entries.RemoveAll(e => e.Ip == ip);
            s_Entries.Add(new SmartDeviceIndexEntry(ip, id, key));
            Persist();
        }
    }

    public static void RemoveEntry(string ip)
    {
        lock (s_Lock)
        {
            s_Entries.RemoveAll(e => e.Ip == ip);
            Persist();
        }
    }

    private static void Persist()
    {
        lock (s_Lock)
        {
            using var fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using var sw = new StreamWriter(fs);
            sw.Write(JsonSerializer.Serialize(s_Entries));
        }
    }

    public static SmartDeviceIndexEntry? GetEntry(string s)
    {
        lock (s_Lock)
        {
            return s_Entries.FirstOrDefault(e => e.Ip == s);
        }
    }
}