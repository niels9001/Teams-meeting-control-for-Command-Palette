using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TeamsExtension.TeamsClient;

internal sealed partial class TeamsWebSocketClient : IDisposable
{
    private const string Host = "127.0.0.1";
    private const int Port = 8124;
    private const string ProtocolVersion = "2.0.0";
    private const string Manufacturer = "Microsoft";
    private const string Device = "CommandPalette";
    private const string App = "TeamsExtension";
    private const string AppVersion = "1.0.0";
    private const int ReceiveBufferSize = 4096;
    private const int ReconnectDelayMs = 3000;

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _receiveCts;
    private int _requestIdCounter;
    private bool _disposed;

    public event EventHandler<MeetingUpdate>? StateChanged;

    public event EventHandler? Connected;

    public event EventHandler? Disconnected;

    public event EventHandler<string>? Error;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);

        var token = await TokenStorage.LoadTokenAsync().ConfigureAwait(false);
        var uri = BuildUri(token);

        _webSocket = new ClientWebSocket();
        _receiveCts = new CancellationTokenSource();

        try
        {
            await _webSocket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
            Connected?.Invoke(this, EventArgs.Empty);
            _ = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), CancellationToken.None);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Connection failed: {ex.Message}");
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_receiveCts is not null)
        {
            await _receiveCts.CancelAsync().ConfigureAwait(false);
            _receiveCts.Dispose();
            _receiveCts = null;
        }

        if (_webSocket is not null)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // Best-effort close
                }
            }

            _webSocket.Dispose();
            _webSocket = null;
        }
    }

    public async Task SendActionAsync(string action, ReactionParameters? parameters = null)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            return;
        }

        var message = new ControlMessage
        {
            RequestId = Interlocked.Increment(ref _requestIdCounter),
            Action = action,
            Parameters = parameters,
        };

        var json = JsonSerializer.Serialize(message, TeamsJsonContext.Default.ControlMessage);
        var buffer = Encoding.UTF8.GetBytes(json);

        try
        {
            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Send failed: {ex.Message}");
        }
    }

    public async Task SendReactionAsync(string reactionType)
    {
        await SendActionAsync(MeetingActions.SendReaction, new ReactionParameters { Type = reactionType }).ConfigureAwait(false);
    }

    public async Task RequestMeetingStateAsync()
    {
        await SendActionAsync(MeetingActions.QueryMeetingState).ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[ReceiveBufferSize];
        var messageBuilder = new StringBuilder();

        while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
        {
            try
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    var message = messageBuilder.ToString();
                    messageBuilder.Clear();
                    ProcessMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (WebSocketException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebSocket receive error: {ex.Message}");
                break;
            }
        }

        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void ProcessMessage(string json)
    {
        try
        {
            // Try parsing as meeting update
            var updateMsg = JsonSerializer.Deserialize(json, TeamsJsonContext.Default.MeetingUpdateMessage);
            if (updateMsg?.MeetingUpdate is not null)
            {
                StateChanged?.Invoke(this, updateMsg.MeetingUpdate);
                return;
            }

            // Try parsing as token refresh
            var tokenMsg = JsonSerializer.Deserialize(json, TeamsJsonContext.Default.TokenRefreshMessage);
            if (tokenMsg?.TokenRefresh is not null)
            {
                _ = TokenStorage.SaveTokenAsync(tokenMsg.TokenRefresh);
                return;
            }

            // Response messages are acknowledged but not acted upon
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"Failed to parse message: {ex.Message}");
        }
    }

    private static Uri BuildUri(string? token)
    {
        var queryParams = new StringBuilder();
        queryParams.Append(System.Globalization.CultureInfo.InvariantCulture, $"protocol-version={Uri.EscapeDataString(ProtocolVersion)}");
        queryParams.Append(System.Globalization.CultureInfo.InvariantCulture, $"&manufacturer={Uri.EscapeDataString(Manufacturer)}");
        queryParams.Append(System.Globalization.CultureInfo.InvariantCulture, $"&device={Uri.EscapeDataString(Device)}");
        queryParams.Append(System.Globalization.CultureInfo.InvariantCulture, $"&app={Uri.EscapeDataString(App)}");
        queryParams.Append(System.Globalization.CultureInfo.InvariantCulture, $"&app-version={Uri.EscapeDataString(AppVersion)}");

        if (!string.IsNullOrEmpty(token))
        {
            queryParams.Append(System.Globalization.CultureInfo.InvariantCulture, $"&token={Uri.EscapeDataString(token)}");
        }

        return new Uri($"ws://{Host}:{Port}?{queryParams}");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _receiveCts?.Cancel();
        _receiveCts?.Dispose();

        if (_webSocket is not null)
        {
            try
            {
                _webSocket.Abort();
                _webSocket.Dispose();
            }
            catch
            {
                // Swallow dispose errors
            }
        }
    }
}
