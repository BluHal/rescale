using System.Text;
using Microsoft.Win32;

namespace Rescale.Interop;

internal static class EdidParser
{
    /// <summary>Reads EDID data from the registry for all connected monitors.</summary>
    public static List<EdidInfo> ReadAllEdid()
    {
        var results = new List<EdidInfo>();
        const string displayKey = @"SYSTEM\CurrentControlSet\Enum\DISPLAY";

        using var display = Registry.LocalMachine.OpenSubKey(displayKey);
        if (display == null) return results;

        foreach (var monitorId in display.GetSubKeyNames())
        {
            using var monitorKey = display.OpenSubKey(monitorId);
            if (monitorKey == null) continue;

            foreach (var instanceId in monitorKey.GetSubKeyNames())
            {
                using var instanceKey = monitorKey.OpenSubKey(instanceId);
                using var paramKey = instanceKey?.OpenSubKey("Device Parameters");
                if (paramKey == null) continue;

                if (paramKey.GetValue("EDID") is not byte[] edid || edid.Length < 128)
                    continue;

                var info = ParseEdid(edid);
                info.RegistryPath = $@"{displayKey}\{monitorId}\{instanceId}";
                results.Add(info);
            }
        }

        return results;
    }

    internal static EdidInfo ParseEdid(byte[] edid)
    {
        var info = new EdidInfo
        {
            Manufacturer = DecodePnpId(edid),
        };

        // Parse descriptor blocks (bytes 54-125, 4 blocks of 18 bytes)
        for (int i = 54; i <= 108; i += 18)
        {
            if (i + 17 >= edid.Length) break;

            // Check for display descriptor (first 3 bytes are 0)
            if (edid[i] == 0 && edid[i + 1] == 0 && edid[i + 2] == 0)
            {
                byte tag = edid[i + 3];
                string text = Encoding.ASCII.GetString(edid, i + 5, 13).TrimEnd('\n', ' ', '\0');

                switch (tag)
                {
                    case 0xFC:
                        info.Model = text;
                        break;
                    case 0xFF:
                        info.Serial = text;
                        break;
                }
            }
        }

        return info;
    }

    private static string DecodePnpId(byte[] edid)
    {
        if (edid.Length < 10) return "";

        ushort id = (ushort)((edid[8] << 8) | edid[9]);
        char c1 = (char)(((id >> 10) & 0x1F) + 'A' - 1);
        char c2 = (char)(((id >> 5) & 0x1F) + 'A' - 1);
        char c3 = (char)((id & 0x1F) + 'A' - 1);
        return $"{c1}{c2}{c3}";
    }
}

internal class EdidInfo
{
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string Serial { get; set; } = "";
    public string RegistryPath { get; set; } = "";
}
