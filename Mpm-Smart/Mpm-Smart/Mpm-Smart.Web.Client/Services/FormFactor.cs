using Mpm_Smart.Shared.Services;

namespace Mpm_Smart.Web.Client.Services;
public class FormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
