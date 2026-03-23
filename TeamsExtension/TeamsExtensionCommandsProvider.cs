using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TeamsExtension;

public partial class TeamsExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly ICommandItem _band;

    public TeamsExtensionCommandsProvider()
    {
        DisplayName = "Teams Meeting Control";
        Icon = Icons.AppLogo;

        var page = new MeetingControlPage(isBandPage: false);
        var bandPage = new MeetingControlPage(isBandPage: true);
        _band = new CommandItem(bandPage) { Title = DisplayName };
        _commands =
        [
            new CommandItem(page)
            {
                Title = "Teams Meeting Control",
                Subtitle = "Control your Teams call",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

    public override ICommandItem[] GetDockBands()
    {
        return [_band];
    }
}
