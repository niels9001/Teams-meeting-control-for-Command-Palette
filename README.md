# Teams Meeting Control for Command Palette

Control your Microsoft Teams meetings directly from the Windows Command Palette. Mute, toggle camera, blur your background, send reactions, raise your hand, and leave calls — all without switching to the Teams window.

![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

## Features

- **Mute / Unmute** — Toggle your microphone on or off
- **Camera On / Off** — Toggle your camera
- **Background Blur** — Blur or unblur your background
- **Leave Call** — Leave the current meeting
- **Raise / Lower Hand** — Get the speaker's attention
- **Reactions** — Send Like, Love, Applause, Laugh, and Wow reactions

All controls appear as a grid in the Command Palette and as quick-action buttons in the Dock when pinned.

## How It Works

This extension communicates with Microsoft Teams via its local third-party device WebSocket API (`ws://127.0.0.1:8124`). This is the same API used by hardware devices like the Elgato Stream Deck to control Teams meetings.

> **Note:** Only the new Microsoft Teams is supported. Teams classic is not supported.

## Setup

Before using this extension, you need to enable the third-party device API in Microsoft Teams:

1. Open **Microsoft Teams** on your desktop
2. Click **Settings and more** (⋯) in the top right, then select **Settings**
3. Go to **Privacy**
4. Under **Third-party app API**, click **Manage API**
5. Turn on the toggle next to **Enable API**

On first use during a meeting, Teams will prompt you with a **"New connection request"** — click **Allow** to pair the extension.

For more details, see the [Microsoft support page](https://support.microsoft.com/en-us/office/connect-to-third-party-devices-in-microsoft-teams-aabca9f2-47bb-407f-9f9b-81a104a883d6).

## Building

### Prerequisites

- Windows 10 (19041) or later
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)
- Visual Studio 2022 (recommended) or the `dotnet` CLI

### Build from command line

```bash
dotnet build TeamsExtension.sln -p:Platform=x64
```

### Deploy

Build and deploy the MSIX package through Visual Studio, or use:

```bash
dotnet publish TeamsExtension.sln -p:Platform=x64 -c Release
```

## Project Structure

```
TeamsExtension/
├── TeamsClient/
│   ├── Models.cs                    — Protocol models & JSON serialization
│   ├── TeamsWebSocketClient.cs      — WebSocket communication
│   ├── TeamsClientManager.cs        — Singleton connection manager
│   └── TokenStorage.cs              — Pairing token persistence
├── Commands/
│   └── MeetingCommands.cs           — All meeting action commands
├── Pages/
│   ├── MeetingControlPage.cs        — Main grid page with action tiles
│   └── SetupPage.cs                 — Setup instructions
├── Icons.cs                         — Centralized icon registry
├── TeamsExtension.cs                — Extension entry point
├── TeamsExtensionCommandsProvider.cs — Command & dock band registration
└── Program.cs                       — COM server host
```

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
