using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using UnityEngine.EventSystems;

public class BabblesCursor : MonoBehaviour
{
    [SerializeField] private float _speed = 1;
    [SerializeField] private bool _defaultHidden;

    public static bool Hidden = false;

    public static bool Click = false;
    public static bool ClickDown = false;

    private static BabblesCursor _instance;

    void Start()
    {
        Hidden = _defaultHidden;
        _instance = this;
    }
    
    
    public static void Move(Vector2 position)
    {
        if (_instance == null)
            return;
        _instance.MoveMouseTo(position);
        _instance.transform.position = position;
    }

    public static Vector2 GetPosition()
    {
        if(_instance == null)
            return new Vector2(Screen.width / 2f, Screen.height / 2f);
        return _instance.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (Hidden.OnceIfTrueGate("cursorBabbles".GetHashCode()))
        {
            transform.localPosition = Vector3.zero;
            GetComponent<Image>().enabled = false;
            Click = false;
            ClickDown = false;
            return;
        }
        if (Hidden)
            return;

        GetComponent<Image>().enabled = true;

        if (InputManager.IsUsingController)
        {
            if (InputManager.GetAxis("Horizontal") != 0)
            {
                transform.position = transform.position.SetX(
                    (transform.position.x + InputManager.GetAxis("Horizontal") * Time.deltaTime * _speed)
                    .Clamp(0, Screen.width));
                MoveMouseTo(transform.position);
                
            }
            if (InputManager.GetAxis("Vertical") != 0)
            {
                transform.position = transform.position.SetY(
                    (transform.position.y + InputManager.GetAxis("Vertical") * Time.deltaTime * _speed)
                    .Clamp(0, Screen.height));
                MoveMouseTo(transform.position);
            }
            if (InputManager.GetButtonDown("UI_Submit"))
            {
                var pos = ScreenToMonitor(transform.position);
                StartClick(pos.x, pos.y);
            }
            if (InputManager.GetButtonUp("UI_Submit"))
            {
                var pos = ScreenToMonitor(transform.position);
                EndClick(pos.x, pos.y);
            }

        }
        else
        {
            transform.position = InputManager.mousePosition;
        }
    }


    private Vector2Int ScreenToMonitor(Vector2 pos)
    {
        pos.y = Screen.height - pos.y; // inverse Y axis to match actual screen
        var offset = GetCursorPosition() - new Vector2(InputManager.mousePosition.x, Screen.height - InputManager.mousePosition.y);
        pos += offset;
        return new Vector2Int(pos.x.Round(), pos.y.Round());
    }
    private void MoveMouseTo(Vector2 pos)
    {
        pos = ScreenToMonitor(pos);
        SetCursorPos(pos.x.Round(), pos.y.Round());
    }

    static uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    static uint MOUSEEVENTF_LEFTUP = 0x0004;
    static uint MOUSEEVENTF_ABSOLUTE = 0x8000;
    static uint MOUSEEVENTF_MOVE = 0x0001;
    [DllImport("user32.dll")]
    static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    private static void StartClick(int x, int y)
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
    }

    private static void EndClick(int x, int y)
    {
        mouse_event(MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
    }

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();


    private int MAKELPARAM(int p, int p_2)
    {
        return ((p_2 << 16) | (p & 0xFFFF));
    }

    private static System.IntPtr GetWindowHandle()
    {
        return GetActiveWindow();
    }

    // The WM_COMMAND message is sent when the user selects a command item from 
    // a menu, when a control sends a notification message to its parent window, 
    // or when an accelerator keystroke is translated.
    public const int WM_KEYDOWN = 0x100;
    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_KEYUP = 0x101;
    public const int WM_COMMAND = 0x111;
    public const int WM_LBUTTONDOWN = 0x201;
    public const int WM_LBUTTONUP = 0x202;
    public const int WM_LBUTTONDBLCLK = 0x203;
    public const int WM_RBUTTONDOWN = 0x204;
    public const int WM_RBUTTONUP = 0x205;
    public const int WM_RBUTTONDBLCLK = 0x206;


    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    
    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Vector2(POINT point)
        {
            return new Vector2(point.X, point.Y);
        }
    }

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    private static Vector2 GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        // NOTE: If you need error handling
        // bool success = GetCursorPos(out lpPoint);
        // if (!success)

        return lpPoint;
    }

}