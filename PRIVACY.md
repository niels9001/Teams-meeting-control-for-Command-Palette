# Privacy Policy

**Teams Meeting Control for Command Palette**

*Last updated: March 23, 2026*

## Overview

Teams Meeting Control for Command Palette is a local desktop extension that communicates with Microsoft Teams running on your computer. Your privacy is important, and this extension is designed to operate entirely on your device.

## Data Collection

**This extension does not collect, store, transmit, or share any personal data.** Specifically:

- No telemetry or analytics are collected
- No usage data is sent to any server
- No personal information is accessed or stored
- No data leaves your device

## How It Works

The extension communicates exclusively with the Microsoft Teams desktop application running on your local machine via a local WebSocket connection (`localhost:8124`). This connection never leaves your computer.

The only data stored locally is a pairing token (in your local app data folder) that allows the extension to reconnect to Teams without requiring re-authorization each time. This token is stored only on your device and is never transmitted externally.

## Third-Party Services

This extension does not integrate with any third-party services, APIs, or cloud infrastructure. All functionality is performed locally between the extension and the Microsoft Teams desktop client.

## Changes to This Policy

If this privacy policy is updated, the changes will be posted in the project's GitHub repository.

## Contact

If you have questions about this privacy policy, please open an issue on the [GitHub repository](https://github.com/niels9001/Teams-meeting-control-for-Command-Palette).
