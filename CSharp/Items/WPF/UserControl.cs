/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region System Namespaces
using System;
using System.Windows.Controls;
#endregion

#region Bentley Namespaces
using BMW = Bentley.MstnPlatformNET.WPF;
using BW = Bentley.Windowing;

#endregion

namespace $rootnamespace$
{
    public partial class $safeitemname$ : UserControl
    {
        #region Bentley DockableWindow
        private static BMW.DockableWindow? _dockableWindow;

internal static void ShowWindow(string unparsed = "")
{
    _ = unparsed; // Avoid unused parameter warning.

    if (_dockableWindow != null)
    {
        _dockableWindow.Focus();
        return;
    }

    _dockableWindow = new BMW.DockableWindow
    {
        Content = new $safeitemname$()
    };

    _dockableWindow.Attach(
        Program.Instance,
        "control",
        new System.Drawing.Size(
            Convert.ToInt32(_dockableWindow.MinWidth),
            Convert.ToInt32(_dockableWindow.MinHeight)));

    _dockableWindow.WindowContent.CanDockVertically = false;
    _dockableWindow.WindowContent.ContentCloseQuery += OnClose;
}

/// <summary>
/// Close and dispose the user control.
/// </summary>
private static void OnClose(object sender, BW.ContentCloseEventArgs e)
{
    e.CloseAction = BW.ContentCloseAction.Dispose;

    if (_dockableWindow != null)
    {
        _dockableWindow.Detach();
        _dockableWindow.Dispose();
        _dockableWindow = null!;
    }
}
    #endregion
}
}