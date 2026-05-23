using System.Windows.Forms;
using AudioBatchConverter.Core;

namespace AudioBatchConverter.UI;

public partial class SettingsForm : Form
{
    public SettingsForm(AppSettings settings)
    {
        InitializeComponent();
        _chkKeepOriginals.Checked = settings.KeepOriginals;
        _txtFfmpegPath.Text = settings.FfmpegPath ?? string.Empty;
    }

    public void ApplyTo(AppSettings settings)
    {
        settings.KeepOriginals = _chkKeepOriginals.Checked;
        settings.FfmpegPath = string.IsNullOrWhiteSpace(_txtFfmpegPath.Text)
            ? null
            : _txtFfmpegPath.Text.Trim();
    }

    private void OnBrowseFfmpegClicked(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Select ffmpeg.exe",
            Filter = "Executable|ffmpeg.exe|All executables|*.exe",
            FileName = "ffmpeg.exe",
        };

        if (!string.IsNullOrWhiteSpace(_txtFfmpegPath.Text) &&
            File.Exists(_txtFfmpegPath.Text))
        {
            dlg.InitialDirectory = Path.GetDirectoryName(_txtFfmpegPath.Text);
        }

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _txtFfmpegPath.Text = dlg.FileName;
        }
    }
}
