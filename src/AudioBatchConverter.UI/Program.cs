using System.Windows.Forms;
using AudioBatchConverter.UI;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

ApplicationConfiguration.Initialize();
Application.Run(new ConversionForm(args));
