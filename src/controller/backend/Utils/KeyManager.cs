using System.Security.Cryptography;

namespace Backend.Utils;

public static class KeyManager
{
    public static async Task CreateKeyIfNotExists(string path)
    {
        if (File.Exists(path))
            return;
        
        var rsaKey = RSA.Create();
        var privateKey = rsaKey.ExportRSAPrivateKey();
        await File.WriteAllBytesAsync(path, privateKey);
    }
    
    public static async Task<RSA> LoadKey(string path)
    {
        var content = await File.ReadAllBytesAsync(path);
        
        var rsaKey = RSA.Create();
        rsaKey.ImportRSAPrivateKey(content, out _);
        return rsaKey;
    }
}