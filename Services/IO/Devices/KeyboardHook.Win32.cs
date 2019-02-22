﻿#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   2019/02/22 12:59
// Modified On:  2019/02/22 12:59
// Modified By:  Alexis

#endregion




using System;
using System.Runtime.InteropServices;

namespace SuperMemoAssistant.Services.IO.Devices
{
  public partial class KeyboardHookService
  {
    #region Constants & Statics

    // ReSharper disable once InconsistentNaming
    public const int WH_KEYBOARD_LL = 13;

    #endregion




    #region Methods

    [DllImport("kernel32.dll",
      CharSet      = CharSet.Auto,
      SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>
    ///   The SetWindowsHookEx function installs an application-defined hook procedure into a
    ///   hook chain. You would install a hook procedure to monitor the system for certain types of
    ///   events. These events are associated either with a specific thread or with all threads in the
    ///   same desktop as the calling thread.
    /// </summary>
    /// <param name="idHook">hook type</param>
    /// <param name="lpfn">hook procedure</param>
    /// <param name="hMod">handle to application instance</param>
    /// <param name="dwThreadId">thread identifier</param>
    /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
    [DllImport("user32.dll",
      CharSet      = CharSet.Auto,
      SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int      idHook,
                                                  HookProc lpfn,
                                                  IntPtr   hMod,
                                                  int      dwThreadId);

    /// <summary>
    ///   The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain
    ///   by the SetWindowsHookEx function.
    /// </summary>
    /// <param name="hHook">handle to hook procedure</param>
    /// <returns>If the function succeeds, the return value is true.</returns>
    [DllImport("user32.dll",
      CharSet      = CharSet.Auto,
      SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hHook);

    /// <summary>
    ///   The CallNextHookEx function passes the hook information to the next hook procedure in
    ///   the current hook chain. A hook procedure can call this function either before or after
    ///   processing the hook information.
    /// </summary>
    /// <param name="hHook">handle to current hook</param>
    /// <param name="code">hook code passed to hook procedure</param>
    /// <param name="wParam">value passed to hook procedure</param>
    /// <param name="lParam">value passed to hook procedure</param>
    /// <returns>If the function succeeds, the return value is true.</returns>
    [DllImport("user32.dll",
      CharSet      = CharSet.Auto,
      SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hHook,
                                                int    code,
                                                IntPtr wParam,
                                                IntPtr lParam);

    [DllImport("user32.dll",
      CharSet = CharSet.Auto)]
    private static extern short GetKeyState(System.Windows.Forms.Keys nVirtKey);

    private static bool GetCapslock()
    {
      return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.CapsLock)) & true;
    }

    private static bool GetNumlock()
    {
      return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.NumLock)) & true;
    }

    private static bool GetScrollLock()
    {
      return Convert.ToBoolean(GetKeyState(System.Windows.Forms.Keys.Scroll)) & true;
    }

    private static bool GetCtrlPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.ControlKey);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private static bool GetAltPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.Menu);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private static bool GetShiftPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.ShiftKey);
      if (state > 1 || state < -1) return true;

      return false;
    }

    private static bool GetMetaPressed()
    {
      int state = GetKeyState(System.Windows.Forms.Keys.LWin);
      if (state > 1 || state < -1) return true;

      state = GetKeyState(System.Windows.Forms.Keys.RWin);
      if (state > 1 || state < -1) return true;

      return false;
    }

    #endregion




    private delegate IntPtr HookProc(int    nCode,
                                     IntPtr wParam,
                                     IntPtr lParam);
  }
}
