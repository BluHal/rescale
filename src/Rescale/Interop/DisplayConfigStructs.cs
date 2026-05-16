using System.Runtime.InteropServices;

namespace Rescale.Interop;

/// <summary>Locally Unique Identifier for display adapters.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct LUID
{
    public uint LowPart;
    public int HighPart;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_RATIONAL
{
    public uint Numerator;
    public uint Denominator;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_PATH_SOURCE_INFO
{
    public LUID AdapterId;
    public uint Id;
    public uint ModeInfoIdx;
    public uint StatusFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_PATH_TARGET_INFO
{
    public LUID AdapterId;
    public uint Id;
    public uint ModeInfoIdx;
    public int OutputTechnology;
    public int Rotation;
    public int Scaling;
    public DISPLAYCONFIG_RATIONAL RefreshRate;
    public int ScanLineOrdering;
    public int TargetAvailable;
    public uint StatusFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_PATH_INFO
{
    public DISPLAYCONFIG_PATH_SOURCE_INFO SourceInfo;
    public DISPLAYCONFIG_PATH_TARGET_INFO TargetInfo;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DISPLAYCONFIG_MODE_INFO
{
    public int InfoType;
    public uint Id;
    public LUID AdapterId;
    public fixed byte Data[48];
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_SOURCE_MODE
{
    public uint Width;
    public uint Height;
    public uint PixelFormat;
    public POINTL Position;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINTL
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_DEVICE_INFO_HEADER
{
    public int Type;
    public uint Size;
    public LUID AdapterId;
    public uint Id;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
{
    public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
    public uint Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
{
    public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
    public uint Value;
    public uint ColorEncoding;
    public uint BitsPerColorChannel;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal unsafe struct DISPLAYCONFIG_TARGET_DEVICE_NAME
{
    public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
    public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS Flags;
    public int OutputTechnology;
    public ushort EdidManufactureId;
    public ushort EdidProductCodeId;
    public uint ConnectorInstance;
    public fixed char MonitorFriendlyDeviceName[64];
    public fixed char MonitorDevicePath[128];
}

[StructLayout(LayoutKind.Sequential)]
internal struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
{
    public uint Value;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal unsafe struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
{
    public DISPLAYCONFIG_DEVICE_INFO_HEADER Header;
    public fixed char ViewGdiDeviceName[32];
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct DEVMODE
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DeviceName;
    public ushort SpecVersion;
    public ushort DriverVersion;
    public ushort Size;
    public ushort DriverExtra;
    public uint Fields;
    public int PositionX;
    public int PositionY;
    public uint DisplayOrientation;
    public uint DisplayFixedOutput;
    public short Color;
    public short Duplex;
    public short YResolution;
    public short TTOption;
    public short Collate;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string FormName;
    public ushort LogPixels;
    public uint BitsPerPel;
    public uint PelsWidth;
    public uint PelsHeight;
    public uint DisplayFlags;
    public uint DisplayFrequency;
    public uint ICMMethod;
    public uint ICMIntent;
    public uint MediaType;
    public uint DitherType;
    public uint Reserved1;
    public uint Reserved2;
    public uint PanningWidth;
    public uint PanningHeight;
}

internal static class DisplayConfigConstants
{
    public const int QDC_ONLY_ACTIVE_PATHS = 0x00000002;
    public const int MODE_INFO_TYPE_SOURCE = 1;
    public const int MODE_INFO_TYPE_TARGET = 2;

    public const uint SDC_APPLY = 0x00000080;
    public const uint SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020;
    public const uint SDC_SAVE_TO_DATABASE = 0x00000200;
    public const uint SDC_ALLOW_CHANGES = 0x00000400;

    public const int DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;
    public const int DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
    public const int DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9;
    public const int DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10;

    public const uint DM_PELSWIDTH = 0x00080000;
    public const uint DM_PELSHEIGHT = 0x00100000;
    public const uint DM_DISPLAYFREQUENCY = 0x00400000;
    public const uint DM_POSITION = 0x00000020;

    public const int ENUM_CURRENT_SETTINGS = -1;
    public const int CDS_UPDATEREGISTRY = 0x00000001;
    public const int DISP_CHANGE_SUCCESSFUL = 0;
    public const int DISP_CHANGE_BADMODE = -2;
}
