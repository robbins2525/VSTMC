/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region System Namespaces
using System;
using System.Drawing;
#endregion

#region Bentley Namespaces
using BMW = Bentley.MstnPlatformNET.WPF;
using BMG = Bentley.MstnPlatformNET.GUI;
#endregion

namespace $rootnamespace$
{
    internal class $safeitemname$ : BMW.DockableToolbar, BMG.IGuiDockable
    {
        private static $safeitemname$? _toolbarInstance;
        private Size _rejectedSize = Size.Empty;

public $safeitemname$(string unparsed = "")
        {
            _ = unparsed; // Avoid unused parameter warning.

            var userControl = new $safeitemname$UC
            {
    VerticalContentAlignment = System.Windows.VerticalAlignment.Center
};

Content = userControl;

            Title = "Dockable Toolbar";
            AttachingToHost += $safeitemname$_AttachingToHost;
            DetachingFromHost += $safeitemname$_DetachingFromHost;

            Attach(Program.Instance, "$safeitemname$Toolbar");

// Setup AutoOpen after calling Attach()
AutoOpen = true;
            // TODO: Change the following AutoOpenKeyin, if necessary.
            AutoOpenKeyin = "mdl silentload $rootnamespace$,,DEFAULTDOMAIN;$rootnamespace$ $safeitemname$Keyin";
        }

/*------------------------------------------------------------------------------------**/
/// <summary>React to the window closing</summary>
/*--------------+---------------+---------------+---------------+---------------+------*/
protected override void OnClosed(EventArgs e)
{
    base.OnClosed(e);

    Detach();
    Dispose();
    _toolbarInstance = null!;
}

/*------------------------------------------------------------------------------------**/
/// <summary>Creates and opens the toolbar</summary>
/*--------------+---------------+---------------+---------------+---------------+------*/
public static void ShowToolbar(string unparsed = "")
{
    _ = unparsed; // Avoid unused parameter warning.

    if (_toolbarInstance != null)
    {
        _toolbarInstance.Focus();
        return;
    }

    _toolbarInstance = new $safeitemname$();
    _toolbarInstance.Show();
}

/*------------------------------------------------------------------------------------**/
/// <summary>Closes the toolbar window</summary>
/*--------------+---------------+---------------+---------------+---------------+------*/
public static void CloseWindow()
{
    _toolbarInstance?.Close();
}

private void $safeitemname$_AttachingToHost(object sender, BMG.AttachingToHostEventArgs e)
        {
            e.AttachPoint = new Point(0, 0);
System.Diagnostics.Debug.WriteLine("$safeitemname$_AttachingToHost");
        }

        private void $safeitemname$_DetachingFromHost(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("$safeitemname$_DetachingFromHost");
        }

        #region IGuiDockable Members

        public bool GetDockedExtent(BMG.GuiDockPosition dockPosition, ref BMG.GuiDockExtent extentFlag, ref Size dockExtent)
{
    dockExtent.Height = CommonDockSize.Height;

    if (dockPosition == BMG.GuiDockPosition.Top ||
        dockPosition == BMG.GuiDockPosition.Bottom)
    {
        dockExtent.Width = (int)ActualWidth;
        extentFlag = BMG.GuiDockExtent.Specified;
    }
    else if (dockPosition == BMG.GuiDockPosition.NotDocked)
    {
        extentFlag = BMG.GuiDockExtent.Specified;
    }
    else
    {
        extentFlag = BMG.GuiDockExtent.InvalidRegion;
    }

    return true;
}

public bool WindowMoving(BMG.WindowMovingCorner corners, ref Size newSize)
{
    newSize.Height = CommonDockSize.Height;

    if (corners != BMG.WindowMovingCorner.LowerRight || _rejectedSize.Equals(newSize))
    {
        _rejectedSize = newSize;
        newSize.Width = (int)ActualWidth;
    }

    return true;
}

        #endregion
    }
}