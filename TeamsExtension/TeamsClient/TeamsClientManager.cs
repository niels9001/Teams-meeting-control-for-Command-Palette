using System;
using System.Threading;
using System.Threading.Tasks;

namespace TeamsExtension.TeamsClient;

internal sealed partial class TeamsClientManager : IDisposable
{
    private static readonly Lazy<TeamsClientManager> LazyInstance = new(() => new TeamsClientManager());

    private readonly TeamsWebSocketClient _client = new();
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _disposed;
    private bool _receivedStateUpdate;

    private TeamsClientManager()
    {
        _client.StateChanged += OnClientStateChanged;
        _client.Connected += OnClientConnected;
        _client.Disconnected += OnClientDisconnected;
        _client.Error += OnClientError;
    }

    public static TeamsClientManager Instance => LazyInstance.Value;

    public MeetingState? CurrentState { get; private set; }

    public MeetingPermissions? CurrentPermissions { get; private set; }

    public bool IsConnected => _client.IsConnected;

    public bool IsInMeeting => CurrentState?.IsInMeeting == true
        || CurrentPermissions?.CanLeave == true;

    public event EventHandler? StateChanged;

    public event EventHandler? ConnectionChanged;

    public async Task EnsureConnectedAsync()
    {
        if (_client.IsConnected || _disposed)
        {
            return;
        }

        await _connectLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!_client.IsConnected && !_disposed)
            {
                _receivedStateUpdate = false;
                await _client.ConnectAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task SendActionAsync(string action)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        await _client.SendActionAsync(action).ConfigureAwait(false);
    }

    public async Task SendReactionAsync(string reactionType)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        await _client.SendReactionAsync(reactionType).ConfigureAwait(false);
    }

    public async Task RefreshStateAsync()
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        await _client.RequestMeetingStateAsync().ConfigureAwait(false);
    }

    public async Task ReconnectAsync()
    {
        await _client.DisconnectAsync().ConfigureAwait(false);
        CurrentState = null;
        CurrentPermissions = null;
        await _client.ConnectAsync().ConfigureAwait(false);
    }

    private void OnClientStateChanged(object? sender, MeetingUpdate update)
    {
        _receivedStateUpdate = true;

        if (update.MeetingState is not null)
        {
            CurrentState = update.MeetingState;
        }

        if (update.MeetingPermissions is not null)
        {
            CurrentPermissions = update.MeetingPermissions;
        }

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClientConnected(object? sender, EventArgs e)
    {
        ConnectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        CurrentState = null;
        CurrentPermissions = null;
        ConnectionChanged?.Invoke(this, EventArgs.Empty);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClientError(object? sender, string message)
    {
        System.Diagnostics.Debug.WriteLine($"Teams client error: {message}");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _client.StateChanged -= OnClientStateChanged;
        _client.Connected -= OnClientConnected;
        _client.Disconnected -= OnClientDisconnected;
        _client.Error -= OnClientError;
        _client.Dispose();
        _connectLock.Dispose();
    }
}
