using System.Diagnostics;
using System.Windows.Forms;
using AudioBatchConverter.Core;

namespace AudioBatchConverter.UI;

public partial class ConversionForm : Form
{
    private static readonly int ImgPending = 0;
    private static readonly int ImgInProgress = 1;
    private static readonly int ImgDone = 2;
    private static readonly int ImgError = 3;

    private readonly string[] _inputPaths;
    private readonly ConversionEngine _engine = new();
    private CancellationTokenSource _cts = new();

    private readonly Dictionary<ConversionJob, TreeNode> _nodeMap = [];
    private bool _hasErrors;
    private string? _firstLogPath;

    public ConversionForm(string[] inputPaths)
    {
        _inputPaths = inputPaths;
        InitializeComponent();

        _btnCancel.Click += (_, _) => CancelConversion();
        _btnOpenLog.Click += (_, _) => OpenLog();

        _engine.JobStatusChanged += OnJobStatusChanged;
        _engine.ProgressChanged += OnProgressChanged;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _ = StartConversionAsync();
    }

    private async Task StartConversionAsync()
    {
        _engine.LoadPaths(_inputPaths);

        if (_engine.Jobs.Count == 0)
        {
            MessageBox.Show(
                "No audio files found in the selected path(s).",
                "Nothing to convert",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Close();
            return;
        }

        BuildTree();
        _progressBar.Maximum = _engine.Jobs.Count;
        _lblCurrentFile.Text = $"Starting conversion of {_engine.Jobs.Count} file(s)…";

        try
        {
            await _engine.RunAsync(_cts.Token);
            OnConversionComplete();
        }
        catch (OperationCanceledException)
        {
            _lblCurrentFile.Text = "Cancelled.";
            _btnCancel.Enabled = false;
        }
    }

    private void BuildTree()
    {
        _treeView.BeginUpdate();
        _treeView.Nodes.Clear();

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
        if (InvokeRequired) { Invoke(() => OnJobStatusChanged(sender, e)); return; }

        if (!_nodeMap.TryGetValue(e.Job, out var node)) return;

        node.ImageIndex = node.SelectedImageIndex = e.Job.Status switch
        {
            JobStatus.InProgress => ImgInProgress,
            JobStatus.Done => ImgDone,
            JobStatus.Error => ImgError,
            _ => ImgPending,
        };

        if (e.Job.Status == JobStatus.InProgress)
        {
            _lblCurrentFile.Text = $"Converting: {Path.GetFileName(e.Job.SourcePath)}";
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
        if (InvokeRequired) { Invoke(() => OnProgressChanged(sender, completed)); return; }
        _progressBar.Value = completed;
        _lblProgress.Text = $"{completed} / {_engine.Jobs.Count}";
    }

    private void OnConversionComplete()
    {
        if (InvokeRequired) { Invoke(OnConversionComplete); return; }
        _lblCurrentFile.Text = _hasErrors
            ? "Done — some files failed. See log for details."
            : "Done — all files converted successfully.";
        _btnCancel.Enabled = false;
    }

    private void CancelConversion()
    {
        _cts.Cancel();
        _btnCancel.Enabled = false;
    }

    private void OpenLog()
    {
        if (_firstLogPath is null || !File.Exists(_firstLogPath)) return;
        Process.Start(new ProcessStartInfo(_firstLogPath) { UseShellExecute = true });
    }

    private static string TruncatePath(string path, int maxLen = 60) =>
        path.Length <= maxLen ? path : "…" + path[^(maxLen - 1)..];
}
