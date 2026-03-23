using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TeamsExtension.Commands;
using TeamsExtension.TeamsClient;

namespace TeamsExtension;

internal sealed partial class MeetingControlPage : ListPage
{
    private readonly bool _isBandPage;
    private bool _initialized;

    public MeetingControlPage(bool isBandPage = false)
    {
        _isBandPage = isBandPage;
        Icon = Icons.AppLogo;
        Title = "Teams Meeting Control";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        if (!_initialized)
        {
            _initialized = true;
            TeamsClientManager.Instance.StateChanged += OnMeetingStateChanged;
            TeamsClientManager.Instance.ConnectionChanged += OnConnectionChanged;
            _ = ConnectAndRefreshAsync();
        }

        var client = TeamsClientManager.Instance;

        // Not connected → show setup guide inline
        if (!client.IsConnected)
        {
            if (_isBandPage)
            {
                return [];
            }

            return GetSetupItems();
        }

        // Connected but not in a meeting
        if (!client.IsInMeeting)
        {
            if (_isBandPage)
            {
                return [];
            }

            return GetNotInMeetingItems();
        }

        // In a meeting → show controls
        return BuildMeetingControlItems(client.CurrentState, client.CurrentPermissions, _isBandPage);
    }

    private static IListItem[] GetSetupItems()
    {
        return
        [
            new ListItem(new SetupPage())
            {
                Title = "Teams API not configured",
                Subtitle = "Set up the Teams third-party device API to get started",
                Icon = new IconInfo("\uE783"), // Error
            },
        ];
    }

    private static IListItem[] GetNotInMeetingItems()
    {
        return
        [
            new ListItem(new NoOpCommand())
            {
                Title = "No active meeting",
                Subtitle = "Join a Teams meeting to see controls here",
                Icon = new IconInfo("\uE8AF"), // Clock / Waiting
            },
        ];
    }

    private static IListItem[] BuildMeetingControlItems(MeetingState? state, MeetingPermissions? perms, bool isBandPage)
    {
        var items = new List<IListItem>();

        // Call controls
        if (perms?.CanToggleMute == true)
        {
            var isMuted = state?.IsMuted == true;
            items.Add(new ListItem(new ToggleMuteCommand())
            {
                Title = isMuted ? "Unmute" : "Mute",
                Icon = isMuted ? Icons.MicOff : Icons.MicOn,
                Section = isBandPage ? string.Empty : "Call controls",
            });
        }

        if (perms?.CanToggleVideo == true)
        {
            var isVideoOn = state?.IsVideoOn == true;
            items.Add(new ListItem(new ToggleVideoCommand())
            {
                Title = isVideoOn ? "Camera off" : "Camera on",
                Icon = isVideoOn ? Icons.CameraOn : Icons.CameraOff,
                Section = isBandPage ? string.Empty : "Call controls",
            });
        }

        if (perms?.CanToggleBlur == true)
        {
            var isBlurred = state?.IsBackgroundBlurred == true;
            items.Add(new ListItem(new ToggleBlurCommand())
            {
                Title = isBlurred ? "Unblur" : "Blur",
                Icon = Icons.Blur,
                Section = isBandPage ? string.Empty : "Call controls",
            });
        }

        if (perms?.CanLeave == true)
        {
            items.Add(new ListItem(new LeaveCallCommand())
            {
                Title = "Leave",
                Icon = Icons.Hangup,
                Section = isBandPage ? string.Empty : "Call controls",
            });
        }

        // Reactions
        if (perms?.CanToggleHand == true)
        {
            var isRaised = state?.IsHandRaised == true;
            items.Add(new ListItem(new ToggleHandCommand())
            {
                Title = isRaised ? "Lower hand" : "Raise hand",
                Icon = isRaised ? Icons.HandRaised : Icons.HandRaise,
                Section = isBandPage ? string.Empty : "Reactions",
            });
        }

        if (perms?.CanReact == true)
        {
            items.Add(new ListItem(new SendReactionCommand(ReactionTypes.Like, "Like", Icons.Like))
            {
                Title = "Like",
                Icon = Icons.Like,
                Section = isBandPage ? string.Empty : "Reactions",
            });
            items.Add(new ListItem(new SendReactionCommand(ReactionTypes.Love, "Love", Icons.Love))
            {
                Title = "Love",
                Icon = Icons.Love,
                Section = isBandPage ? string.Empty : "Reactions",
            });
            items.Add(new ListItem(new SendReactionCommand(ReactionTypes.Applause, "Applause", Icons.Applause))
            {
                Title = "Applause",
                Icon = Icons.Applause,
                Section = isBandPage ? string.Empty : "Reactions",
            });
            items.Add(new ListItem(new SendReactionCommand(ReactionTypes.Laugh, "Laugh", Icons.Laugh))
            {
                Title = "Laugh",
                Icon = Icons.Laugh,
                Section = isBandPage ? string.Empty : "Reactions",
            });
            items.Add(new ListItem(new SendReactionCommand(ReactionTypes.Wow, "Wow", Icons.Wow))
            {
                Title = "Wow",
                Icon = Icons.Wow,
                Section = isBandPage ? string.Empty : "Reactions",
            });
        }

        return items.ToArray();
    }

    private static async System.Threading.Tasks.Task ConnectAndRefreshAsync()
    {
        try
        {
            await TeamsClientManager.Instance.EnsureConnectedAsync().ConfigureAwait(false);
        }
        catch
        {
            // Connection failures are handled by the StateChanged/ConnectionChanged events
        }
    }

    private void OnMeetingStateChanged(object? sender, EventArgs e)
    {
        RaiseItemsChanged();
    }

    private void OnConnectionChanged(object? sender, EventArgs e)
    {
        RaiseItemsChanged();
    }
}
