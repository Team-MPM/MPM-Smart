using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Frontend.Services;

public class RT(ControllerConnectionManager connectionManager, ISnackbar snackbar)
{
    private readonly ConcurrentDictionary<string, HubConnection> m_OpenConnections = new();

    public HubConnection GetConnection(string connectionName)
    {
        if (!m_OpenConnections.TryGetValue(connectionName, out var value))
            throw new InvalidOperationException("Connection not found");

        return value;
    }
    
    public HubConnection GetOrOpenConnection(string connectionName, string hubUrl,
        Action<HubConnection> configureHandlers) =>
        m_OpenConnections.AddOrUpdate(connectionName, _ =>
        {
            var connection = connectionManager.GetSignalRClient(path: hubUrl)
                .WithAutomaticReconnect()
                .Build();

            connection.KeepAliveInterval = TimeSpan.FromSeconds(5);
            connection.ServerTimeout = TimeSpan.FromSeconds(60);
            connection.HandshakeTimeout = TimeSpan.FromSeconds(10);

            connection.Closed += error =>
            {
                if (error != null)
                {
                    snackbar.Add("Connection closed unexpectedly: " + error.Message, Severity.Error);
                }
                else
                {
                    snackbar.Add("Connection closed unexpectedly", Severity.Warning);
                }

                return Task.CompletedTask;
            };

            connection.Reconnecting += _ =>
            {
                snackbar.Add("Connection lost, reconnecting...", Severity.Warning);
                return Task.CompletedTask;
            };

            connection.Reconnected += connectionId =>
            {
                snackbar.Add("Connection re-established: " + connectionId, Severity.Success);
                return Task.CompletedTask;
            };

            configureHandlers(connection);
            
            connection.StartAsync();

            return connection;
        }, (_, existingConnection) =>
        {
            configureHandlers(existingConnection);
            existingConnection.StartAsync();
            return existingConnection;
        });

    public async Task CloseConnection(string connectionName)
    {
        if (!m_OpenConnections.TryGetValue(connectionName, out var value))
            return;

        await value.StopAsync();
        await value.DisposeAsync();
        m_OpenConnections.Remove(connectionName, out _);
    }
}