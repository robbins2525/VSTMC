using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using VSTMC.Options;

namespace VSTMC
{
    public partial class OptionsControlPage : UserControl
    {
        #region Fields

        // Nullable for designer support. In runtime, we set it before using.
        internal OptionsPage? msceOptionsPage;

        private bool isMSCEToolTipShown = false;
        private bool isSDKToolTipShown = false;
        private bool isMDLToolTipShown = false;
        private bool isBatchToolTipShown = false;

        #endregion

        #region Constructors

        // Required for WinForms designer
        public OptionsControlPage()
        {
            InitializeComponent();
        }

        // Preferred runtime ctor (optional if you set OptionsPage property after creation)
        public OptionsControlPage(OptionsPage optionsPage) : this()
        {
            msceOptionsPage = optionsPage;
        }

        #endregion

        /// <summary>
        /// Initialize control fields from the OptionsPage.
        /// Safe even if the designer calls it accidentally.
        /// </summary>
        public void Initialize()
        {
            // If you want designer safety, keep this:
            if (msceOptionsPage is null)
                return;

            var page = OptionsPage; // guaranteed non-null here

            tbBentley_AppPath.Text = page.Bentley_AppPath ?? string.Empty;
            tbSDKPath.Text = page.MSCESDKPath ?? string.Empty;
            tbMDLAPPSPath.Text = page.MDLAPPSPath ?? string.Empty;
            tbBuildPath.Text = page.BentleyBuildFilePath ?? string.Empty;

            _ = page.BentleyApp;
        }


        //public void Initialize()
        //{
        //    if (msceOptionsPage is null) return;
        //        tbBentley_AppPath.Text = msceOptionsPage.Bentley_AppPath ?? string.Empty;
        //        tbSDKPath.Text = msceOptionsPage.MSCESDKPath ?? string.Empty;
        //        tbMDLAPPSPath.Text = msceOptionsPage.MDLAPPSPath ?? string.Empty;
        //        tbBuildPath.Text = msceOptionsPage.BentleyBuildFilePath ?? string.Empty; // kept for parity with your original code
        //        _ = msceOptionsPage.BentleyApp; }




        /// <summary>
        /// Gets or sets the reference to the underlying OptionsPage object.
        /// NOTE: Set this before calling Initialize() if you use the parameterless ctor.
        /// </summary>
        public OptionsPage OptionsPage
        {
            get => msceOptionsPage ?? throw new InvalidOperationException("OptionsPage was not set.");
            set => msceOptionsPage = value;
        }
        #region OptionPageMSCE Methods

        private void OptionsControlsMSCE_Load(object sender, EventArgs e)
        {
            // If OptionsPage not set yet, don’t crash; show empty UI.
            if (msceOptionsPage is null)
                return;

            SetMSCEComboBox();
            SetMDLPadlockImage();
            SetBatchPadlockImage();

            btnMDLAppsFolder.Enabled = PathExist(tbMDLAPPSPath);
            btnBentleyBuildFolder.Enabled = IsFileExist(tbBuildPath);
        }

        private void OptionsControlsMSCE_MouseMove(object sender, MouseEventArgs e)
        {
            if (tbBentley_AppPath.TextLength > 60)
            {
                if (tbBentley_AppPath == GetChildAtPoint(e.Location))
                {
                    if (!isMSCEToolTipShown)
                    {
                        toolTip1.Show(tbBentley_AppPath.Text, tbBentley_AppPath, 0, 25, 5000);
                        isMSCEToolTipShown = true;
                    }
                }
                else
                {
                    toolTip1.Hide(tbBentley_AppPath);
                    isMSCEToolTipShown = false;
                }
            }

            if (tbSDKPath.TextLength > 60)
            {
                if (tbSDKPath == GetChildAtPoint(e.Location))
                {
                    if (!isSDKToolTipShown)
                    {
                        toolTip1.Show(tbSDKPath.Text, tbSDKPath, 0, 25, 5000);
                        isSDKToolTipShown = true;
                    }
                }
                else
                {
                    toolTip1.Hide(tbSDKPath);
                    isSDKToolTipShown = false;
                }
            }

            if (!tbMDLAPPSPath.Enabled)
            {
                if (tbMDLAPPSPath.TextLength > 60)
                {
                    if (tbMDLAPPSPath == GetChildAtPoint(e.Location))
                    {
                        if (!isMDLToolTipShown)
                        {
                            toolTip1.Show(tbMDLAPPSPath.Text, tbMDLAPPSPath, 0, 25, 5000);
                            isMDLToolTipShown = true;
                        }
                    }
                    else
                    {
                        toolTip1.Hide(tbMDLAPPSPath);
                        isMDLToolTipShown = false;
                    }
                }
            }

            if (!tbBuildPath.Enabled)
            {
                if (tbBuildPath.TextLength > 60)
                {
                    if (tbBuildPath == GetChildAtPoint(e.Location))
                    {
                        if (!isBatchToolTipShown)
                        {
                            toolTip1.Show(tbBuildPath.Text, tbBuildPath, 0, 25, 5000);
                            isBatchToolTipShown = true;
                        }
                    }
                    else
                    {
                        toolTip1.Hide(tbBuildPath);
                        isBatchToolTipShown = false;
                    }
                }
            }
        }

        private void CboBentleyProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            try
            {
                ICollection bentleyKeyCollection = BentleyDataCollector.BentleyProducts.Keys;
                ICollection bentleyValueCollection = BentleyDataCollector.BentleyProducts.Values;

                string[] bentleyKeys = new string[BentleyDataCollector.BentleyProducts.Count];
                string[] bentleyValues = new string[BentleyDataCollector.BentleyProducts.Count];
                bentleyKeyCollection.CopyTo(bentleyKeys, 0);
                bentleyValueCollection.CopyTo(bentleyValues, 0);

                for (int i = 0; i < BentleyDataCollector.BentleyProducts.Count; i++)
                {
                    if (!string.IsNullOrEmpty(cboBentleyProduct.Text) && bentleyKeys[i] == cboBentleyProduct.Text)
                    {
                        tbBentley_AppPath.Text = bentleyValues[i];
                        break;
                    }
                }

                tbSDKPath.Text = BentleyDataCollector.GetSDKPath(tbBentley_AppPath.Text);
                msceOptionsPage.BentleyApp = BentleyDataCollector.GetBentleyApp(tbBentley_AppPath.Text);

                if (!msceOptionsPage.MDLAPPSLock)
                    tbMDLAPPSPath.Text = BentleyDataCollector.GetMdlappsPath(tbBentley_AppPath.Text);

                if (!msceOptionsPage.BatchLock)
                    tbBuildPath.Text = BentleyDataCollector.BentleyBuildBatchFilePath(msceOptionsPage.BentleyApp);
            }
            catch
            {
                // Do nothing when exception occurs.
            }
        }

        private void TbBentley_AppPath_TextChanged(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            msceOptionsPage.Bentley_AppPath = tbBentley_AppPath.Text;
        }

        private void TbSDKPath_TextChanged(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            msceOptionsPage.MSCESDKPath = tbSDKPath.Text;
        }

        private void TbMDLAPPSPath_TextChanged(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            msceOptionsPage.MDLAPPSPath = tbMDLAPPSPath.Text;
            btnMDLAppsFolder.Enabled = PathExist(tbMDLAPPSPath);
        }

        private void TbBuildPath_TextChanged(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            msceOptionsPage.BentleyBuildFilePath = tbBuildPath.Text;
            btnBentleyBuildFolder.Enabled = IsFileExist(tbBuildPath);
        }

        private static void TryOpenFolder(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            if (!Directory.Exists(folderPath))
                return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private static void TryOpenFileDirectory(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string? dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                return;

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = dir,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void BtnMSFolder_Click(object sender, EventArgs e) => TryOpenFolder(tbBentley_AppPath.Text);
        private void BtnSDKFolder_Click(object sender, EventArgs e) => TryOpenFolder(tbSDKPath.Text);
        private void BtnMDLAppsFolder_Click(object sender, EventArgs e) => TryOpenFolder(tbMDLAPPSPath.Text);
        private void BtnBentleyBuildFolder_Click(object sender, EventArgs e) => TryOpenFileDirectory(tbBuildPath.Text);

        private void BtnMDLAPPSPathBrowser_Click(object sender, EventArgs e)
        {
            tbMDLAPPSPath.Text = GetFolderPath("MDLAPPS Build Path", Environment.SpecialFolder.MyComputer, tbMDLAPPSPath.Text);
        }

        private void BtnBatchBrowser_Click(object sender, EventArgs e)
        {
            tbBuildPath.Text = GetFilePath(tbBuildPath);
        }

        private void BtnMDLAPPSLock_Click(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            if (btnMDLAPPSLock.Image?.Tag?.ToString() == nameof(Properties.Resources.OpenedPadlock))
            {
                btnMDLAPPSLock.Image = Properties.Resources.LockedPadlock;
                btnMDLAPPSLock.Image.Tag = nameof(Properties.Resources.LockedPadlock);
                btnMDLAPPSPathBrowser.Enabled = false;
                tbMDLAPPSPath.Enabled = false;
            }
            else
            {
                btnMDLAPPSLock.Image = Properties.Resources.OpenedPadlock;
                btnMDLAPPSLock.Image.Tag = nameof(Properties.Resources.OpenedPadlock);
                btnMDLAPPSPathBrowser.Enabled = true;
                tbMDLAPPSPath.Enabled = true;
            }

            msceOptionsPage.MDLAPPSLock = !msceOptionsPage.MDLAPPSLock;
        }

        private void BtnBatchLock_Click(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            if (btnBatchLock.Image?.Tag?.ToString() == nameof(Properties.Resources.OpenedPadlock))
            {
                btnBatchLock.Image = Properties.Resources.LockedPadlock;
                btnBatchLock.Image.Tag = nameof(Properties.Resources.LockedPadlock);
                btnBatchBrowser.Enabled = false;
                tbBuildPath.Enabled = false;
            }
            else
            {
                btnBatchLock.Image = Properties.Resources.OpenedPadlock;
                btnBatchLock.Image.Tag = nameof(Properties.Resources.OpenedPadlock);
                btnBatchBrowser.Enabled = true;
                tbBuildPath.Enabled = true;
            }

            msceOptionsPage.BatchLock = !msceOptionsPage.BatchLock;
        }

        private void TbMDLAPPSPath_MouseHover(object sender, EventArgs e)
        {
            if (tbMDLAPPSPath.TextLength > 60)
                toolTip1.Show(tbMDLAPPSPath.Text, tbMDLAPPSPath, 0, 25, 3000);
        }

        private void TbMDLAPPSPath_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(tbMDLAPPSPath);

        private void TbBuildPath_MouseHover(object sender, EventArgs e)
        {
            if (tbBuildPath.TextLength > 60)
                toolTip1.Show(tbBuildPath.Text, tbBuildPath, 0, 25, 3000);
        }

        private void TbBuildPath_MouseLeave(object sender, EventArgs e) => toolTip1.Hide(tbBuildPath);

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (msceOptionsPage is null)
                return;

            tbBentley_AppPath.Text = BentleyDataCollector.Bentley_AppPath();

            foreach (var item in BentleyDataCollector.BentleyProducts)
            {
                if (tbBentley_AppPath.Text == item.Value)
                {
                    cboBentleyProduct.Text = item.Key;
                }
            }

            tbSDKPath.Text = BentleyDataCollector.GetSDKPath(tbBentley_AppPath.Text);
            msceOptionsPage.BentleyApp = BentleyDataCollector.GetBentleyApp(tbBentley_AppPath.Text);

            if (!msceOptionsPage.MDLAPPSLock)
                tbMDLAPPSPath.Text = BentleyDataCollector.GetMdlappsPath(tbBentley_AppPath.Text);

            if (!msceOptionsPage.BatchLock)
                tbBuildPath.Text = BentleyDataCollector.BentleyBuildBatchFilePath(msceOptionsPage.BentleyApp);
        }

        #endregion

        #region Methods

        private void SetMSCEComboBox()
        {
            if (msceOptionsPage is null)
                return;

            if (BentleyDataCollector.BentleyProducts.Count == 0)
                _ = BentleyDataCollector.Bentley_AppPath();

            cboBentleyProduct.Items.Clear();

            foreach (var item in BentleyDataCollector.BentleyProducts)
                cboBentleyProduct.Items.Add(item.Key);

            int i = 0;
            foreach (var item in BentleyDataCollector.BentleyProducts)
            {
                if (msceOptionsPage.Bentley_AppPath == item.Value)
                {
                    cboBentleyProduct.SelectedIndex = i;
                    break;
                }
                i += 1;
            }
        }

        private string GetFolderPath(string title, Environment.SpecialFolder environment, string path)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = title;
                dialog.ShowNewFolderButton = false;
                dialog.RootFolder = environment;

                if (dialog.ShowDialog() == DialogResult.OK)
                    path = dialog.SelectedPath + "\\";
            }

            return path;
        }

        private string GetFilePath(TextBox textBox)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "Batch file (*.bat)|*.bat";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Title = "Select Bentley build batch file";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    return openFileDialog1.FileName;

                return textBox.Text;
            }
        }

        private bool PathExist(TextBox textbox)
        {
            string text = textbox.Text ?? string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                if (Directory.Exists(text) && text.EndsWith("\\", StringComparison.Ordinal))
                {
                    errorProvider1.SetError(textbox, string.Empty);
                    textbox.ForeColor = Color.Black;
                    return true;
                }

                if (text != "Not Installed")
                    errorProvider1.SetError(textbox, "Path not Found");
                else
                {
                    errorProvider1.SetError(textbox, string.Empty);
                    textbox.ForeColor = Color.Black;
                    return false;
                }

                textbox.ForeColor = Color.Red;
            }

            return false;
        }

        private bool IsFileExist(TextBox textbox)
        {
            string text = textbox.Text ?? string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                if (File.Exists(text))
                {
                    errorProvider1.SetError(textbox, string.Empty);
                    textbox.ForeColor = Color.Black;
                    return true;
                }

                if (text != "Not Installed")
                    errorProvider1.SetError(textbox, "Path not Found");
                else
                {
                    errorProvider1.SetError(textbox, string.Empty);
                    textbox.ForeColor = Color.Black;
                    return false;
                }

                textbox.ForeColor = Color.Red;
            }

            return false;
        }

        private void SetMDLPadlockImage()
        {
            if (msceOptionsPage is null)
                return;

            if (msceOptionsPage.MDLAPPSLock)
            {
                btnMDLAPPSLock.Image = Properties.Resources.LockedPadlock;
                btnMDLAPPSLock.Image.Tag = nameof(Properties.Resources.LockedPadlock);
                btnMDLAPPSPathBrowser.Enabled = false;
                tbMDLAPPSPath.Enabled = false;
            }
            else
            {
                btnMDLAPPSLock.Image = Properties.Resources.OpenedPadlock;
                btnMDLAPPSLock.Image.Tag = nameof(Properties.Resources.OpenedPadlock);
                btnMDLAPPSPathBrowser.Enabled = true;
                tbMDLAPPSPath.Enabled = true;
            }
        }

        private void SetBatchPadlockImage()
        {
            if (msceOptionsPage is null)
                return;

            if (msceOptionsPage.BatchLock)
            {
                btnBatchLock.Image = Properties.Resources.LockedPadlock;
                btnBatchLock.Image.Tag = nameof(Properties.Resources.LockedPadlock);
                btnBatchBrowser.Enabled = false;
                tbBuildPath.Enabled = false;
            }
            else
            {
                btnBatchLock.Image = Properties.Resources.OpenedPadlock;
                btnBatchLock.Image.Tag = nameof(Properties.Resources.OpenedPadlock);
                btnBatchBrowser.Enabled = true;
                tbBuildPath.Enabled = true;
            }
        }

        #endregion

        #region Properties

        

        #endregion
    }
}
