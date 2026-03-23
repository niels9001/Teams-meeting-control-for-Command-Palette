using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TeamsExtension;

internal sealed partial class SetupPage : ContentPage
{
    private static readonly string SetupMarkdown = """
        # Teams Extension — Setup Guide

        This extension lets you control Microsoft Teams meetings directly from the Command Palette.
        You can **mute/unmute**, **toggle camera**, **blur your background**, **raise your hand**,
        **send reactions**, and **leave the call**.

        ## How to Enable

        Before using this extension, you need to enable the third-party device API in Microsoft Teams:

        1. Open **Microsoft Teams** on your desktop
        2. Click **Settings and more** (⋯) in the top right, then select **Settings**
        3. Go to **Privacy**
        4. Under **Third-party app API**, click **Manage API**
        5. Turn on the toggle next to **Enable API**

        ## First Connection

        When you use the extension for the first time during a meeting, Teams will show a
        **"New connection request"** prompt:

        - Click **Allow** to pair the extension with Teams
        - The extension will be added to your **Allowed apps and devices** list

        ## Troubleshooting

        - If **Manage API** is greyed out, your IT administrator may have disabled third-party device pairing
        - Make sure you are using the **new Microsoft Teams** (not Teams classic)
        - The extension only works while you are in a Teams meeting or call

        > For more details, see the [Microsoft support page](https://support.microsoft.com/en-us/office/connect-to-third-party-devices-in-microsoft-teams-aabca9f2-47bb-407f-9f9b-81a104a883d6).
        """;

    public SetupPage()
    {
        Icon = new IconInfo("\uE946"); // Info
        Name = "Setup Guide";
        Title = "Teams Extension Setup";

        Commands =
        [
            new CommandContextItem(
                title: "Open Microsoft Support Page",
                name: "Open Support Page",
                subtitle: "View detailed setup instructions on Microsoft's website",
                result: CommandResult.Dismiss(),
                action: () =>
                {
                    _ = Windows.System.Launcher.LaunchUriAsync(
                        new System.Uri("https://support.microsoft.com/en-us/office/connect-to-third-party-devices-in-microsoft-teams-aabca9f2-47bb-407f-9f9b-81a104a883d6"));
                }),
        ];
    }

    public override IContent[] GetContent() => [new MarkdownContent(SetupMarkdown)];
}
