using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TeamsExtension.TeamsClient;

namespace TeamsExtension.Commands;

internal abstract partial class MeetingActionCommand : InvokableCommand
{
    protected static TeamsClientManager Client => TeamsClientManager.Instance;

    protected static ICommandResult ShowActionToast(string message)
    {
        var toast = new ToastStatusMessage(message);
        toast.Show();
        return CommandResult.KeepOpen();
    }

    protected static ICommandResult ShowErrorToast(string message)
    {
        var toast = new ToastStatusMessage(new StatusMessage() { Message = message, State = MessageState.Error });
        toast.Show();
        return CommandResult.KeepOpen();
    }
}

internal sealed partial class ToggleMuteCommand : MeetingActionCommand
{
    public ToggleMuteCommand()
    {
        Name = "Toggle mute";
        Id = "teams.toggle-mute";
        Icon = Icons.MicOn;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Client.SendActionAsync(MeetingActions.ToggleMute);
            var state = Client.CurrentState;
            return ShowActionToast(state?.IsMuted == true ? "Unmuted" : "Muted");
        }
        catch (Exception ex)
        {
            return ShowErrorToast($"Failed to toggle mute: {ex.Message}");
        }
    }
}

internal sealed partial class ToggleVideoCommand : MeetingActionCommand
{
    public ToggleVideoCommand()
    {
        Name = "Toggle camera";
        Id = "teams.toggle-video";
        Icon = Icons.CameraOn;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Client.SendActionAsync(MeetingActions.ToggleVideo);
            var state = Client.CurrentState;
            return ShowActionToast(state?.IsVideoOn == true ? "Camera Off" : "Camera On");
        }
        catch (Exception ex)
        {
            return ShowErrorToast($"Failed to toggle camera: {ex.Message}");
        }
    }
}

internal sealed partial class ToggleBlurCommand : MeetingActionCommand
{
    public ToggleBlurCommand()
    {
        Name = "Toggle background blur";
        Id = "teams.toggle-blur";
        Icon = Icons.Blur;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Client.SendActionAsync(MeetingActions.ToggleBackgroundBlur);
            var state = Client.CurrentState;
            return ShowActionToast(state?.IsBackgroundBlurred == true ? "Blur Off" : "Blur On");
        }
        catch (Exception ex)
        {
            return ShowErrorToast($"Failed to toggle blur: {ex.Message}");
        }
    }
}

internal sealed partial class ToggleHandCommand : MeetingActionCommand
{
    public ToggleHandCommand()
    {
        Name = "Raise/lower hand";
        Id = "teams.toggle-hand";
        Icon = Icons.HandRaise;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Client.SendActionAsync(MeetingActions.ToggleHand);
            var state = Client.CurrentState;
            return ShowActionToast(state?.IsHandRaised == true ? "Hand Lowered" : "Hand Raised");
        }
        catch (Exception ex)
        {
            return ShowErrorToast($"Failed to toggle hand: {ex.Message}");
        }
    }
}

internal sealed partial class LeaveCallCommand : MeetingActionCommand
{
    public LeaveCallCommand()
    {
        Name = "Leave call";
        Id = "teams.leave-call";
        Icon = Icons.Hangup;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Client.SendActionAsync(MeetingActions.LeaveCall);
            return ShowActionToast("Left the call");
        }
        catch (Exception ex)
        {
            return ShowErrorToast($"Failed to leave call: {ex.Message}");
        }
    }
}

internal sealed partial class SendReactionCommand : MeetingActionCommand
{
    private readonly string _reactionType;

    public SendReactionCommand(string reactionType, string displayName, IconInfo icon)
    {
        _reactionType = reactionType;
        Name = displayName;
        Id = $"teams.react-{reactionType}";
        Icon = icon;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _ = Client.SendReactionAsync(_reactionType);
            return ShowActionToast($"Sent {Name}");
        }
        catch (Exception ex)
        {
            return ShowErrorToast($"Failed to send reaction: {ex.Message}");
        }
    }
}
