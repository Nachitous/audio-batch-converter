using System.Drawing;
using System.Windows.Forms;

namespace AudioBatchConverter.UI;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null!;

    private CheckBox _chkKeepOriginals = null!;
    private TextBox _txtFfmpegPath = null!;
    private Button _btnBrowseFfmpeg = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ── Conversion group ──────────────────────────────────────────────────

        _chkKeepOriginals = new CheckBox
        {
            Text = "Keep original files after successful conversion",
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 4),
        };

        var grpConversion = new GroupBox
        {
            Text = "Conversion",
            Padding = new Padding(10, 6, 10, 10),
            Dock = DockStyle.Top,
            Height = 60,
        };
        grpConversion.Controls.Add(_chkKeepOriginals);
        _chkKeepOriginals.Location = new System.Drawing.Point(10, 24);

        // ── FFmpeg group ──────────────────────────────────────────────────────

        var lblFfmpeg = new Label
        {
            Text = "Path (leave empty to use bundled ffmpeg.exe):",
            AutoSize = true,
            Location = new System.Drawing.Point(10, 22),
        };

        _txtFfmpegPath = new TextBox
        {
            Location = new System.Drawing.Point(10, 42),
            Width = 310,
            PlaceholderText = "Default (bundled)",
        };

        _btnBrowseFfmpeg = new Button
        {
            Text = "Browse…",
            Location = new System.Drawing.Point(328, 41),
            Width = 72,
            Height = 24,
        };
        _btnBrowseFfmpeg.Click += OnBrowseFfmpegClicked;

        var grpFfmpeg = new GroupBox
        {
            Text = "FFmpeg",
            Padding = new Padding(10, 6, 10, 10),
            Dock = DockStyle.Top,
            Height = 80,
        };
        grpFfmpeg.Controls.AddRange([lblFfmpeg, _txtFfmpegPath, _btnBrowseFfmpeg]);

        // ── OK / Cancel ───────────────────────────────────────────────────────

        _btnOk = new Button
        {
            Text = "OK",
            Width = 80,
            Height = 28,
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        _btnCancel = new Button
        {
            Text = "Cancel",
            Width = 80,
            Height = 28,
            DialogResult = DialogResult.Cancel,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        var btnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 40,
            Padding = new Padding(8, 4, 8, 4),
        };
        btnRow.Controls.Add(_btnCancel);
        btnRow.Controls.Add(_btnOk);

        // ── form ──────────────────────────────────────────────────────────────

        Controls.Add(btnRow);
        Controls.Add(grpConversion);
        Controls.Add(grpFfmpeg);

        Text = "Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new System.Drawing.Size(420, 200);
        Font = new Font("Segoe UI", 9f);
        StartPosition = FormStartPosition.CenterParent;
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;
    }
}
