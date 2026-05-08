/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#nullable enable

#region System Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#endregion

#region Bentley Namespaces
using BDPN = Bentley.DgnPlatformNET;
using BG = Bentley.GeometryNET;
using BM = Bentley.MstnPlatformNET;
using BMW = Bentley.MstnPlatformNET.WinForms;
using BW = Bentley.Windowing;
#endregion

namespace $rootnamespace$
{
    //Select Design in Configuration to easily switch to System.Windows.Form designer
#if DESIGN
    [System.ComponentModel.DesignerCategory("designer")]
    public partial class $safeitemrootname$ : Form
#else
    [System.ComponentModel.DesignerCategory("code")]
public partial class $safeitemrootname$ : BMW.Adapter
#endif
    {
       internal $safeitemrootname$()
       {
            InitializeComponent();
       }

#region MicroStation
public static $safeitemrootname$? $safeitemrootname$Form { get; set; }
private Bentley.Windowing.WindowContent? _windowContent;

/// <summary>
/// Show the form if it is not already displayed
/// </summary>
internal void ShowForm(string unparsed = "")
{
    _ = unparsed; // To avoid compiler warning about unused parameter.

    if (null != $safeitemrootname$Form)
    {
        $safeitemrootname$Form.Focus();
        return;
    }

    $safeitemrootname$Form = new $safeitemrootname$();
    $safeitemrootname$Form.AttachAsTopLevelForm(Program.Instance, true);

    $safeitemrootname$Form.AutoOpen = true;
    $safeitemrootname$Form.AutoOpenKeyin = "mdl load $safeitemrootname$";

    $safeitemrootname$Form.NETDockable = true;
    Bentley.Windowing.WindowManager windowManager =
                Bentley.Windowing.WindowManager.GetForMicroStation();
    $safeitemrootname$Form._windowContent =
        windowManager.DockPanel($safeitemrootname$Form, $safeitemrootname$Form.Name, $safeitemrootname$Form.Name,
        Bentley.Windowing.DockLocation.Floating);

    $safeitemrootname$Form._windowContent.CanDockHorizontally = false; // limit to left and right docking
    $safeitemrootname$Form._windowContent.ContentCloseQuery += OnClose;
}

/// <summary>
/// Override Form OnClosed method.
/// </summary>
/// <param name="e"></param>
protected override void OnClosed(System.EventArgs e)
{
    $safeitemrootname$Form?._windowContent?.Close();
    base.OnClosed(e);
}

/// <summary>
/// Close and dispose the form.
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private static void OnClose(object sender, Bentley.Windowing.ContentCloseEventArgs e)
{
    e.CloseAction = Bentley.Windowing.ContentCloseAction.Dispose;
    $safeitemrootname$Form?._windowContent?.Hide();
    if (null != $safeitemrootname$Form)
    {
        $safeitemrootname$Form.DetachFromMicroStation();
        $safeitemrootname$Form.Dispose();
        $safeitemrootname$Form = null!;
    }
}

/// <summary>
/// Adjust to controls when the form changes size
/// </summary>
private void $safeitemrootname$_SizeChanged(object sender, System.EventArgs e)
{
    if (this.DesignMode)
    {
        System.Diagnostics.Debug.Assert(!this.DesignMode, "Do not use SetFormSizes in design mode.");
return;
    }
}
#endregion

    }
}