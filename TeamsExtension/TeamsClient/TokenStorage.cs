using System;
using System.IO;
using System.Threading.Tasks;

namespace TeamsExtension.TeamsClient;

internal static class TokenStorage
{
    private static readonly string TokenFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TeamsExtension",
        "pairing-token.txt");

    public static async Task<string?> LoadTokenAsync()
    {
        try
        {
            if (File.Exists(TokenFilePath))
            {
                return await File.ReadAllTextAsync(TokenFilePath).ConfigureAwait(false);
            }
        }
        catch
        {
            // Ignore read errors — treat as no token
        }

        return null;
    }

    public static async Task SaveTokenAsync(string token)
    {
        try
        {
            var directory = Path.GetDirectoryName(TokenFilePath);
            if (directory is not null)
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(TokenFilePath, token).ConfigureAwait(false);
        }
        catch
        {
            // Best-effort save — non-critical if it fails
        }
    }
}
