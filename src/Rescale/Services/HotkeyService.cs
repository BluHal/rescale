using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Rescale.Interop;

namespace Rescale.Services;

/// <summary>Registers and dispatches global keyboard hotkeys via Win32 RegisterHotKey/UnregisterHotKey.</summary>
public sealed class HotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private readonly Dictionary<int, Action> _registeredHotkeys = new();
    private nint _hwnd;
    private HwndSource? _source;
    private int _nextId = 1;

    /// <summary>Attaches the hotkey listener to a window's message pump.</summary>
    /// <param name="window">The WPF window whose HWND will receive WM_HOTKEY messages.</param>
    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.Handle;
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);
    }

    /// <summary>Registers a global hotkey and associates it with a callback.</summary>
    /// <param name="hotkeyString">Hotkey string (e.g. "Ctrl+Alt+1").</param>
    /// <param name="callback">Action to invoke when the hotkey is pressed.</param>
    /// <returns>The registration ID, or null if registration failed.</returns>
    public int? Register(string hotkeyString, Action callback)
    {
        if (!TryParseHotkey(hotkeyString, out var modifiers, out var vk))
            return null;

        int id = _nextId++;
        if (!NativeMethods.RegisterHotKey(_hwnd, id, modifiers, vk))
            return null;

        _registeredHotkeys[id] = callback;
        return id;
    }

    /// <summary>Unregisters a previously registered hotkey by its ID.</summary>
    public void Unregister(int id)
    {
        NativeMethods.UnregisterHotKey(_hwnd, id);
        _registeredHotkeys.Remove(id);
    }

    /// <summary>Unregisters all currently registered hotkeys.</summary>
    public void UnregisterAll()
    {
        foreach (var id in _registeredHotkeys.Keys.ToList())
            Unregister(id);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && _registeredHotkeys.TryGetValue((int)wParam, out var callback))
        {
            callback();
            handled = true;
        }
        return 0;
    }

    /// <summary>Parses a hotkey string like "Ctrl+Alt+F1" into Win32 modifier flags and virtual key code.</summary>
    /// <param name="hotkey">The hotkey string to parse.</param>
    /// <param name="modifiers">Output Win32 modifier flags (MOD_CONTROL, MOD_ALT, etc.).</param>
    /// <param name="vk">Output virtual key code.</param>
    /// <returns>True if parsing succeeded and a valid key was found.</returns>
    public static bool TryParseHotkey(string hotkey, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        if (string.IsNullOrWhiteSpace(hotkey))
            return false;

        var parts = hotkey.Split('+', StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var upper = part.ToUpperInvariant();
            switch (upper)
            {
                case "CTRL" or "CONTROL":
                    modifiers |= 0x0002; // MOD_CONTROL
                    break;
                case "ALT":
                    modifiers |= 0x0001; // MOD_ALT
                    break;
                case "SHIFT":
                    modifiers |= 0x0004; // MOD_SHIFT
                    break;
                case "WIN":
                    modifiers |= 0x0008; // MOD_WIN
                    break;
                default:
                    if (Enum.TryParse<Key>(part, true, out var key))
                    {
                        vk = (uint)KeyInterop.VirtualKeyFromKey(key);
                    }
                    else if (upper.Length == 1 && char.IsLetterOrDigit(upper[0]))
                    {
                        vk = (uint)upper[0];
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }
        }

        return vk != 0;
    }

    /// <summary>Unregisters all hotkeys and detaches from the window message pump.</summary>
    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
    }
}
