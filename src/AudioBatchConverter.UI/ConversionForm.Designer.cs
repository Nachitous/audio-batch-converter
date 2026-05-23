using System.Windows.Forms;
using System.Drawing;

namespace AudioBatchConverter.UI;

partial class ConversionForm
{
    private System.ComponentModel.IContainer components = null!;

    private MenuStrip _menuStrip = null!;
    private ToolStripMenuItem _menuItemOpenFolder = null!;
    private TreeView _treeView = null!;
    private Label _lblCurrentFile = null!;
    private ProgressBar _progressBar = null!;
    private Label _lblProgress = null!;
    private Button _btnCancel = null!;
    private Button _btnOpenLog = null!;
    private ImageList _statusImages = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _statusImages = new ImageList(components) { ImageSize = new Size(16, 16) };
        _statusImages.Images.Add(MakeDot(Color.Gray));        // 0 pending
        _statusImages.Images.Add(MakeDot(Color.DodgerBlue));  // 1 in-progress
        _statusImages.Images.Add(MakeDot(Color.SeaGreen));    // 2 done
        _statusImages.Images.Add(MakeDot(Color.Crimson));     // 3 error

        // ── menu bar ──────────────────────────────────────────────────────────

        _menuItemOpenFolder = new ToolStripMenuItem("Open Folder…")
        {
            ShortcutKeys = Keys.Control | Keys.O,
        };
        _menuItemOpenFolder.Click += OnBrowseClicked;

        var menuItemExit = new ToolStripMenuItem("Exit");
        menuItemExit.Click += (_, _) => Close();

        var menuItemFile = new ToolStripMenuItem("File");
        menuItemFile.DropDownItems.Add(_menuItemOpenFolder);
        menuItemFile.DropDownItems.Add(new ToolStripSeparator());
        menuItemFile.DropDownItems.Add(menuItemExit);

        _menuStrip = new MenuStrip();
        _menuStrip.Items.Add(menuItemFile);

        // ── tree view ─────────────────────────────────────────────────────────

        _treeView = new TreeView
        {
            Dock = DockStyle.Fill,
            ImageList = _statusImages,
            ShowLines = true,
            FullRowSelect = true,
            Font = new Font("Segoe UI", 9f),
            BorderStyle = BorderStyle.None,
        };

        // ── bottom status bar ─────────────────────────────────────────────────

        _lblCurrentFile = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9f),
            Text = "Ready — File > Open Folder to begin",
            TextAlign = ContentAlignment.MiddleLeft,
        };

        _progressBar = new ProgressBar
        {
            Width = 160,
            Height = 18,
            Minimum = 0,
            Maximum = 100,
            Style = ProgressBarStyle.Continuous,
            Margin = new Padding(0, 11, 6, 0),
        };

        _lblProgress = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = SystemColors.GrayText,
            Text = "",
            Margin = new Padding(0, 12, 8, 0),
        };

        _btnCancel = new Button
        {
            Text = "Cancel",
            Width = 90,
            Height = 32,
            Margin = new Padding(0, 6, 6, 6),
        };

        _btnOpenLog = new Button
        {
            Text = "Open Log",
            Width = 90,
            Height = 32,
            Margin = new Padding(0, 6, 0, 6),
            Visible = false,
        };

        var statusBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Height = 44,
            Padding = new Padding(8, 0, 8, 0),
            BackColor = SystemColors.ControlLight,
        };
        statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // label
        statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // bar
        statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // count
        statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // cancel
        statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // log
        statusBar.Controls.Add(_lblCurrentFile, 0, 0);
        statusBar.Controls.Add(_progressBar, 1, 0);
        statusBar.Controls.Add(_lblProgress, 2, 0);
        statusBar.Controls.Add(_btnCancel, 3, 0);
        statusBar.Controls.Add(_btnOpenLog, 4, 0);

        // ── main layout ───────────────────────────────────────────────────────

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.Controls.Add(_treeView, 0, 0);
        mainLayout.Controls.Add(statusBar, 0, 1);

        Controls.Add(mainLayout);
        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;

        Text = "Audio Batch Converter";
        Size = new Size(720, 520);
        MinimumSize = new Size(500, 350);
        Font = new Font("Segoe UI", 9f);
        StartPosition = FormStartPosition.CenterScreen;
    }

    private static Bitmap MakeDot(Color color)
    {
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 2, 2, 12, 12);
        return bmp;
    }
}
