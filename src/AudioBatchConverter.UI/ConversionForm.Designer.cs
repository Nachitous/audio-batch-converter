using System.Windows.Forms;
using System.Drawing;

namespace AudioBatchConverter.UI;

partial class ConversionForm
{
    private System.ComponentModel.IContainer components = null!;

    private SplitContainer _splitContainer = null!;
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

        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 340,
            Panel1MinSize = 200,
            Panel2MinSize = 280,
        };

        _treeView = new TreeView
        {
            Dock = DockStyle.Fill,
            ImageList = _statusImages,
            ShowLines = true,
            FullRowSelect = true,
            Font = new Font("Segoe UI", 9f),
        };

        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 5,
            ColumnCount = 1,
        };
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _lblCurrentFile = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 40,
            Font = new Font("Segoe UI", 9f),
            Text = "Scanning…",
            TextAlign = ContentAlignment.MiddleLeft,
        };

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 22,
            Minimum = 0,
            Maximum = 100,
            Style = ProgressBarStyle.Continuous,
        };

        _lblProgress = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Height = 24,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = SystemColors.GrayText,
            Text = "",
            TextAlign = ContentAlignment.MiddleLeft,
        };

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
        };

        _btnCancel = new Button
        {
            Text = "Cancel",
            Width = 90,
            Height = 32,
            Margin = new Padding(0, 8, 8, 0),
        };

        _btnOpenLog = new Button
        {
            Text = "Open Log",
            Width = 90,
            Height = 32,
            Margin = new Padding(0, 8, 0, 0),
            Visible = false,
        };

        btnPanel.Controls.AddRange([_btnCancel, _btnOpenLog]);

        rightPanel.Controls.Add(_lblCurrentFile, 0, 0);
        rightPanel.Controls.Add(_progressBar, 0, 1);
        rightPanel.Controls.Add(_lblProgress, 0, 2);
        rightPanel.Controls.Add(btnPanel, 0, 3);

        _splitContainer.Panel1.Controls.Add(_treeView);
        _splitContainer.Panel2.Controls.Add(rightPanel);

        Controls.Add(_splitContainer);

        Text = "Audio Batch Converter";
        Size = new Size(800, 500);
        MinimumSize = new Size(600, 400);
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
