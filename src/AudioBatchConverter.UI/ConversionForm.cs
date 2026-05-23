using System.Diagnostics;
using System.Windows.Forms;
using AudioBatchConverter.Core;

namespace AudioBatchConverter.UI;

public partial class ConversionForm : Form
{
    private static readonly int ImgPending    = 0;
    private static readonly int ImgInProgress = 1;
    private static readonly int ImgDone       = 2;
    private static readonly int ImgError      = 3;

    private string[] _inputPaths;
    private readonly ConversionEngine _engine = new();
    private CancellationTokenSource _cts = new();
    private AppSettings _settings = AppSettings.Load();

    private readonly Dictionary<ConversionJob, TreeNode> _nodeMap = [];
    private bool _hasErrors;
    private string? _firstLogPath;
    private bool _conversionComplete;

    public ConversionForm(string[] inputPaths)
    {
        _inputPaths = inputPaths;
        InitializeComponent();

        var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        this.Icon = icon ?? this.Icon;
        _notifyIcon.Icon = icon;

        _btnCancel.Click += (_, _) => { if (_conversionComplete) Close(); else CancelConversion(); };
        _btnOpenLog.Click += (_, _) => OpenLog();

        _engine.JobStatusChanged += OnJobStatusChanged;
        _engine.ProgressChanged += OnProgressChanged;

        ApplySettings();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_inputPaths.Contains("--pick-folder"))
        {
            OnBrowseClicked(this, EventArgs.Empty);
        }
        else if (_inputPaths.Length > 0)
        {
            RunConversion();
        }
    }

    private void OnBrowseClicked(object? sender, EventArgs e)
    {
        var folder = NativeFolderPicker.Pick(IntPtr.Zero, "Select a folder to convert");
        if (folder is null)
        {
            return;
        }

        _inputPaths = [folder];
        _hasErrors = false;
        _firstLogPath = null;
        _btnOpenLog.Visible = false;
        RunConversion();
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        using var dlg = new SettingsForm(_settings);
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        dlg.ApplyTo(_settings);
        _settings.Save();
        ApplySettings();
    }

    private void ApplySettings()
    {
        _engine.KeepOriginals = _settings.KeepOriginals;

        var ffmpegPath = string.IsNullOrWhiteSpace(_settings.FfmpegPath)
            ? FfmpegConverter.FindFfmpeg()
            : _settings.FfmpegPath;

        _engine.SetFfmpegPath(ffmpegPath);
    }

    // async void is correct here — top-level fire-and-forget on the UI thread
    private async void RunConversion()
    {
        _conversionComplete = false;
        _btnCancel.Text = "Cancel";
        _btnCancel.Enabled = true;
        _cts = new CancellationTokenSource();

        try
        {
            await ScanAndBuildTreeAsync();

            if (_engine.Jobs.Count == 0)
            {
                MessageBox.Show(
                    "No audio files found in the selected path(s).",
                    "Nothing to convert",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                _lblCurrentFile.Text = "Ready — File > Open Folder to begin";
                _btnCancel.Enabled = false;
                return;
            }

            _progressBar.Maximum = _engine.Jobs.Count;
            _lblCurrentFile.Text = $"Starting — {_engine.Jobs.Count} file(s)";

            await _engine.RunAsync(_cts.Token);
            OnConversionComplete();
        }
        catch (OperationCanceledException)
        {
            _lblCurrentFile.Text = "Cancelled.";
            _btnCancel.Enabled = false;
        }
        catch (Exception ex)
        {
            _lblCurrentFile.Text = $"Error: {ex.Message}";
            _btnCancel.Enabled = false;
        }
    }

    private async Task ScanAndBuildTreeAsync()
    {
        _lblCurrentFile.Text = "Scanning…";
        _treeView.Nodes.Clear();
        _nodeMap.Clear();

        await Task.Run(() => _engine.LoadPaths(_inputPaths));

        _treeView.BeginUpdate();
        var dirNodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);

        foreach (var job in _engine.Jobs)
        {
            var dir = Path.GetDirectoryName(job.SourcePath) ?? "";
            if (!dirNodes.TryGetValue(dir, out var dirNode))
            {
                dirNode = new TreeNode(TruncatePath(dir)) { ToolTipText = dir };
                _treeView.Nodes.Add(dirNode);
                dirNodes[dir] = dirNode;
            }

            var fileNode = new TreeNode(Path.GetFileName(job.SourcePath))
            {
                ImageIndex = ImgPending,
                SelectedImageIndex = ImgPending,
                ToolTipText = job.SourcePath,
            };
            dirNode.Nodes.Add(fileNode);
            _nodeMap[job] = fileNode;
        }

        _treeView.ExpandAll();
        _treeView.EndUpdate();
    }

    private void OnJobStatusChanged(object? sender, JobStatusChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnJobStatusChanged(sender, e));
            return;
        }

        if (!_nodeMap.TryGetValue(e.Job, out var node))
        {
            return;
        }

        node.ImageIndex = node.SelectedImageIndex = e.Job.Status switch
        {
            JobStatus.InProgress => ImgInProgress,
            JobStatus.Done       => ImgDone,
            JobStatus.Error      => ImgError,
            _                    => ImgPending,
        };

        if (e.Job.Status == JobStatus.InProgress)
        {
            _lblCurrentFile.Text = Path.GetFileName(e.Job.SourcePath);
            node.EnsureVisible();
        }

        if (e.Job.Status == JobStatus.Error)
        {
            _hasErrors = true;
            _firstLogPath ??= ErrorLogger.GetLogPath(e.Job.SourcePath);
            _btnOpenLog.Visible = true;
        }
    }

    private void OnProgressChanged(object? sender, int completed)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnProgressChanged(sender, completed));
            return;
        }
        _progressBar.Value = Math.Min(completed, _progressBar.Maximum);
        _lblProgress.Text = $"{completed} / {_engine.Jobs.Count}";
    }

    private void OnConversionComplete()
    {
        if (InvokeRequired)
        {
            Invoke(OnConversionComplete);
            return;
        }

        _conversionComplete = true;
        _btnCancel.Text = "Exit";
        _btnCancel.Enabled = true;

        string title = "Audio Batch Converter";
        string savings = _engine.TotalBytesSaved > 0
            ? $"\n{FormatBytes(_engine.TotalBytesSaved)} saved."
            : "";
        string message = _hasErrors
            ? $"Conversion finished — some files failed.{savings}\nCheck the log for details."
            : $"All files converted successfully.{savings}";
        ToolTipIcon tipIcon = _hasErrors ? ToolTipIcon.Warning : ToolTipIcon.Info;

        _lblCurrentFile.Text = _hasErrors ? "Done — some files failed." : "Done — all files converted.";

        ShowBalloon(title, message, tipIcon);
        MessageBox.Show(message, title, MessageBoxButtons.OK,
            _hasErrors ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
    }

    private void ShowBalloon(string title, string text, ToolTipIcon icon)
    {
        _notifyIcon.Visible = true;
        _notifyIcon.ShowBalloonTip(5000, title, text, icon);
        _notifyIcon.BalloonTipClosed  += HideTrayIcon;
        _notifyIcon.BalloonTipClicked += HideTrayIcon;
    }

    private void HideTrayIcon(object? sender, EventArgs e)
    {
        _notifyIcon.BalloonTipClosed  -= HideTrayIcon;
        _notifyIcon.BalloonTipClicked -= HideTrayIcon;
        _notifyIcon.Visible = false;
    }

    private void CancelConversion()
    {
        _cts.Cancel();
        _btnCancel.Enabled = false;
    }

    private void OpenLog()
    {
        if (_firstLogPath is null || !File.Exists(_firstLogPath))
        {
            return;
        }
        Process.Start(new ProcessStartInfo(_firstLogPath) { UseShellExecute = true });
    }

    private static string TruncatePath(string path, int maxLen = 70) =>
        path.Length <= maxLen ? path : "…" + path[^(maxLen - 1)..];

    private static string FormatBytes(long bytes) =>
        bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
            < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):0.#} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):0.##} GB",
        };
}
