using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TeamsExtension;

public partial class TeamsExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly ICommandItem _band;
    private readonly MeetingControlPage _page;

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
        foreach (var item in _page.GetItems())
        {
            if (item.Command?.Id == id)
            {
                return item;
            }
        }

        return null;
    }
}
