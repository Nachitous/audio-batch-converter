using System.Windows.Forms;
using AudioBatchConverter.UI;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

ApplicationConfiguration.Initialize();

string[] paths;

if (args.Contains("--pick-folder") || args.Length == 0)
{
    paths = PromptForPaths();
}
else
{
    paths = args;
}

if (paths.Length == 0)
    return;

Application.Run(new ConversionForm(paths));

static string[] PromptForPaths()
{
    using var dlg = new FolderBrowserDialog
    {
        Description = "Select a folder to convert",
        UseDescriptionForTitle = true,
    };
    return dlg.ShowDialog() == DialogResult.OK
        ? [dlg.SelectedPath]
        : [];
}
