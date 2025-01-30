using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using PluginBase.Services.Permissions;

namespace PluginBase.Services.General;

public class HubBase : Hub
{
    public bool IsConnected { get; set; }
    public bool IsAuthenticated { get; set; }
    
    private List<Claim>? m_UserClaims;
    private readonly Dictionary<string, bool> m_Permissions = new(); 
    private static readonly ConcurrentDictionary<string, Timer> TokenExpirationTimers = new();
    
    public event Action? OnAuthStateChanged;
    
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
        }
        
        if (IsAuthenticated)
        {
            m_UserClaims = user!.Claims.ToList();
        }
        
        IsConnected = true;
        await ScheduleTokenRefresh();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        IsConnected = false;
        IsAuthenticated = false;
        m_UserClaims?.Clear();
        
        if (TokenExpirationTimers.TryRemove(Context.ConnectionId, out var timer))
        {
            timer.Dispose();
        }
        
        return Task.CompletedTask;
    }
    
    private async Task ScheduleTokenRefresh()
    {
        var expClaim = m_UserClaims?.FirstOrDefault(c => c.Type == "exp")?.Value;
        
        if (expClaim == null) 
            return;

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim));
        var refreshTime = expirationTime.AddMinutes(-2);
        var delay = refreshTime - DateTimeOffset.UtcNow;

        if (delay.TotalMilliseconds <= 0)
        {
            await SessionExpired();
            return;
        }

        var timer = new Timer(_ =>
        {
            if (IsConnected)
            {
                Task.Run(SessionExpired);
            }
        }, null, delay, Timeout.InfiniteTimeSpan);

        TokenExpirationTimers[Context.ConnectionId] = timer;
    }
    
    public void AuthStateChanged()
    {
        m_Permissions.Clear();
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            m_UserClaims = Context.User.Claims.ToList();
        }
        else
        {
            IsAuthenticated = false;
            m_UserClaims?.Clear();
        }
        
        OnAuthStateChanged?.Invoke();
    }

    public async Task SessionExpired()
    {
        await Clients.Caller.SendAsync("TokenExpired");
    }
    
    [HubMethodName(nameof(SessionRenewed))]
    public void SessionRenewed()
    {
        AuthStateChanged();
    }

    
    public bool CheckPermission(string permission)
    {
        if (m_Permissions.TryGetValue(permission, out var hasPermission))
        {
            return hasPermission;
        }

        if (m_UserClaims is null)
            return false;
        
        hasPermission = PermissionHandler.HasAccess(m_UserClaims, permission);
        m_Permissions.Add(permission, hasPermission);
        
        return hasPermission;
    }
    
    public async Task PushMessage(string message)
    {
        await Clients.Caller.SendAsync("PushMessage", message);
    }
    
    public async Task PushWarning(string warning)
    {
        await Clients.Caller.SendAsync("PushWarning", warning);
    }
    
    public async Task PushError(string error)
    {
        await Clients.Caller.SendAsync("PushError", error);
    }
}