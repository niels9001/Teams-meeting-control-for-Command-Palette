using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeamsExtension.TeamsClient;

// JSON serialization context for AOT compatibility
[JsonSerializable(typeof(MeetingUpdateMessage))]
[JsonSerializable(typeof(TokenRefreshMessage))]
[JsonSerializable(typeof(ResponseMessage))]
[JsonSerializable(typeof(ControlMessage))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class TeamsJsonContext : JsonSerializerContext;

// Outgoing messages

internal sealed class ControlMessage
{
    public int RequestId { get; set; }

    public string Action { get; set; } = string.Empty;

    public ReactionParameters? Parameters { get; set; }
}

internal sealed class ReactionParameters
{
    public string Type { get; set; } = string.Empty;
}

// Incoming messages

internal sealed class MeetingUpdateMessage
{
    public MeetingUpdate? MeetingUpdate { get; set; }
}

internal sealed class MeetingUpdate
{
    public MeetingState? MeetingState { get; set; }

    public MeetingPermissions? MeetingPermissions { get; set; }
}

internal sealed class MeetingState
{
    public bool IsMuted { get; set; }

    public bool IsVideoOn { get; set; }

    public bool IsHandRaised { get; set; }

    public bool IsInMeeting { get; set; }

    public bool IsRecordingOn { get; set; }

    public bool IsBackgroundBlurred { get; set; }
}

internal sealed class MeetingPermissions
{
    public bool CanToggleMute { get; set; }

    public bool CanToggleVideo { get; set; }

    public bool CanToggleHand { get; set; }

    public bool CanToggleBlur { get; set; }

    public bool CanToggleRecord { get; set; }

    public bool CanLeave { get; set; }

    public bool CanReact { get; set; }

    public bool CanToggleShareTray { get; set; }

    public bool CanToggleChat { get; set; }

    public bool CanStopSharing { get; set; }

    public bool CanPair { get; set; }
}

internal sealed class TokenRefreshMessage
{
    public string? TokenRefresh { get; set; }
}

internal sealed class ResponseMessage
{
    public int RequestId { get; set; }

    public string? Response { get; set; }
}

// Well-known action strings
internal static class MeetingActions
{
    public const string ToggleMute = "toggle-mute";
    public const string ToggleVideo = "toggle-video";
    public const string ToggleBackgroundBlur = "toggle-background-blur";
    public const string ToggleHand = "toggle-hand";
    public const string ToggleRecording = "toggle-recording";
    public const string LeaveCall = "leave-call";
    public const string SendReaction = "send-reaction";
    public const string QueryMeetingState = "query-meeting-state";
}

internal static class ReactionTypes
{
    public const string Like = "like";
    public const string Love = "love";
    public const string Applause = "applause";
    public const string Laugh = "laugh";
    public const string Wow = "wow";
}
