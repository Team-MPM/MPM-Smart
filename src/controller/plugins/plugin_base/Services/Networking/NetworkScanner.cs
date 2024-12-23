using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PluginBase.Services.Networking;

public record struct IpSubnet(string Ip, string SubnetPrefix, bool IsIpv6);

public partial class NetworkScanner(ILogger<NetworkScanner> logger)
{
    public async Task<List<IpSubnet>> GetLocalSubnetsAsync()
    {
        var subnets = new List<IpSubnet>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ip",
                Arguments = "a",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        logger.LogInformation("'ip a' output: {output}", output);

        foreach (Match match in Ipv4Regex().Matches(output))
            if (match.Groups.Count > 2)
                subnets.Add(new IpSubnet(match.Groups[1].Value, match.Groups[2].Value, false));

        foreach (Match match in Ipv6Regex().Matches(output))
            if (match.Groups.Count > 2)
                subnets.Add(new IpSubnet(match.Groups[1].Value, match.Groups[2].Value, true));

        return subnets;
    }

    public async IAsyncEnumerable<string> ScanTcpAsync(uint port)
    {
        var subnets = await GetLocalSubnetsAsync();

        foreach (var subnet in subnets)
        {
            if (subnet.Ip.StartsWith("::1") || subnet.Ip.StartsWith("127.") || subnet.Ip.StartsWith("fe80::"))
            {
                continue;
            }

            logger.LogInformation("Scanning subnet: {subnet}", subnet);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nmap",
                    Arguments = $"-p {port} --open {subnet.Ip}/{subnet.SubnetPrefix} {(subnet.IsIpv6 ? "-6" : "")}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            logger.LogInformation("Nmap output: {output}", output);

            var regex = NmapRegex();

            foreach (Match match in regex.Matches(output))
                if (match.Groups.Count > 1)
                    yield return match.Groups[1].Value;
        }
    }

    [GeneratedRegex(@"inet (\d+\.\d+\.\d+\.\d+)/(\d+)")]
    private static partial Regex Ipv4Regex();

    [GeneratedRegex(@"inet6? ([\da-fA-F:.]+)/(\d+)")]
    private static partial Regex Ipv6Regex();

    [GeneratedRegex(@"Nmap scan report for (.+)")]
    private static partial Regex NmapRegex();
}