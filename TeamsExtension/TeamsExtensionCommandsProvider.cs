using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TeamsExtension.Commands;
using TeamsExtension.TeamsClient;

namespace TeamsExtension;

public partial class TeamsExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly ICommandItem _band;
    private readonly MeetingControlPage _page;
    private readonly Dictionary<string, ICommandItem> _commandItemCache = [];

    public TeamsExtensionCommandsProvider()
    {
        DisplayName = "Teams Meeting Control for Command Palette";
        Icon = Icons.AppLogo;

        _page = new MeetingControlPage(isBandPage: false);
        var bandPage = new MeetingControlPage(isBandPage: true);
        _band = new CommandItem(bandPage) { Title = DisplayName };
        _commands =
        [
            new CommandItem(_page)
            {
                Title = "Teams Meeting Control",
                Subtitle = "Control your Teams call",
            },
        ];

        BuildCommandCache();

        // Signal readiness after construction so CmdPal re-resolves
        // any saved dock pins that may have been queried too early.
        _ = Task.Run(async () =>
        {
            await Task.Delay(500);
            RaiseItemsChanged();
        });
    }

    private void BuildCommandCache()
    {
        RegisterCommand(new ToggleMuteCommand());
        RegisterCommand(new ToggleVideoCommand());
        RegisterCommand(new ToggleBlurCommand());
        RegisterCommand(new ToggleHandCommand());
        RegisterCommand(new LeaveCallCommand());
        RegisterCommand(new SendReactionCommand(ReactionTypes.Like, "Like", Icons.Like));
        RegisterCommand(new SendReactionCommand(ReactionTypes.Love, "Love", Icons.Love));
        RegisterCommand(new SendReactionCommand(ReactionTypes.Applause, "Applause", Icons.Applause));
        RegisterCommand(new SendReactionCommand(ReactionTypes.Laugh, "Laugh", Icons.Laugh));
        RegisterCommand(new SendReactionCommand(ReactionTypes.Wow, "Wow", Icons.Wow));
    }

    private void RegisterCommand(InvokableCommand cmd)
    {
        _commandItemCache[cmd.Id] = new CommandItem(cmd) { Title = cmd.Name, Icon = cmd.Icon };
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

    public override ICommandItem[] GetDockBands()
    {
        return [_band];
    }

    public override ICommandItem? GetCommandItem(string id)
    {
        return _commandItemCache.TryGetValue(id, out var item) ? item : null;
    }
}
