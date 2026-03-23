using Microsoft.CommandPalette.Extensions.Toolkit;

namespace TeamsExtension;

internal static class Icons
{
    // Microphone
    public static IconInfo MicOn { get; } = IconHelpers.FromRelativePaths(
        "Assets\\mic-on.light.svg",
        "Assets\\mic-on.dark.svg");

    public static IconInfo MicOff { get; } = IconHelpers.FromRelativePaths(
        "Assets\\mic-off.light.svg",
        "Assets\\mic-off.dark.svg");

    // Camera
    public static IconInfo CameraOn { get; } = IconHelpers.FromRelativePaths(
        "Assets\\camera-on.light.svg",
        "Assets\\camera-on.dark.svg");

    public static IconInfo CameraOff { get; } = IconHelpers.FromRelativePaths(
        "Assets\\camera-off.light.svg",
        "Assets\\camera-off.dark.svg");

    // Background blur
    public static IconInfo Blur { get; } = IconHelpers.FromRelativePaths(
        "Assets\\blur.light.svg",
        "Assets\\blur.dark.svg");

    // Leave call
    public static IconInfo Hangup { get; } = IconHelpers.FromRelativePath(
        "Assets\\hangup.svg");

    // Reactions
    public static IconInfo Like { get; } = IconHelpers.FromRelativePath("Assets\\like.png");

    public static IconInfo Love { get; } = IconHelpers.FromRelativePath("Assets\\love.png");

    public static IconInfo Applause { get; } = IconHelpers.FromRelativePath("Assets\\applause.png");

    public static IconInfo Laugh { get; } = IconHelpers.FromRelativePath("Assets\\laugh.png");

    public static IconInfo Wow { get; } = IconHelpers.FromRelativePath("Assets\\surprised.png");

    // Hand
    public static IconInfo HandRaise { get; } = IconHelpers.FromRelativePaths(
        "Assets\\raise.light.svg",
        "Assets\\raise.dark.svg");

    public static IconInfo HandRaised { get; } = IconHelpers.FromRelativePath("Assets\\hand.png");
}
