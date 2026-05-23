using AudioBatchConverter.Worker;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<WorkerSettings>(builder.Configuration.GetSection("Worker"));
builder.Services.AddSingleton<ConversionTracker>();
builder.Services.AddHostedService<ConversionWorker>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// ── filesystem browser ────────────────────────────────────────────────────────

app.MapGet("/api/browse", (string? path, IOptions<WorkerSettings> opts) =>
{
    var s = opts.Value;
    var root = NormalisePath(s.BrowserRoot);
    var target = string.IsNullOrEmpty(path) ? root : NormalisePath(path);

    if (!IsWithinRoot(target, root))
    {
        return Results.BadRequest("Path is outside allowed root");
    }

    if (!Directory.Exists(target))
    {
        return Results.NotFound("Directory not found");
    }

    var extensions = s.GetExtensionSet();

    var dirs = SafeGetDirectories(target)
        .OrderBy(d => Path.GetFileName(d), StringComparer.OrdinalIgnoreCase)
        .Select(d => new { name = Path.GetFileName(d), path = d })
        .ToList();

    var audioFiles = SafeGetFiles(target)
        .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
        .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
        .Select(f => new
        {
            name = Path.GetFileName(f),
            path = f,
            converted = File.Exists(Path.ChangeExtension(f, ".mp3")),
        })
        .ToList();

    var crumbs = BuildBreadcrumbs(target, root);
    var parent = target != root ? Path.GetDirectoryName(target) : null;

    return Results.Ok(new { path = target, parent, breadcrumbs = crumbs, dirs, audioFiles });
});

// ── count audio files in a folder before converting ───────────────────────────

app.MapGet("/api/preview", (string path, IOptions<WorkerSettings> opts) =>
{
    var s = opts.Value;
    var root = NormalisePath(s.BrowserRoot);
    var target = NormalisePath(path);

    if (!IsWithinRoot(target, root) || !Directory.Exists(target))
    {
        return Results.BadRequest();
    }

    var extensions = s.GetExtensionSet();
    var enumOpts = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };

    var files = Directory.EnumerateFiles(target, "*", enumOpts)
        .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
        .ToList();

    return Results.Ok(new
    {
        total = files.Count,
        unconverted = files.Count(f => !File.Exists(Path.ChangeExtension(f, ".mp3"))),
    });
});

// ── queue a folder for conversion ─────────────────────────────────────────────

app.MapPost("/api/convert", (ConvertRequest req, ConversionTracker tracker, IOptions<WorkerSettings> opts) =>
{
    var s = opts.Value;
    var root = NormalisePath(s.BrowserRoot);
    var target = NormalisePath(req.Path);

    if (!IsWithinRoot(target, root) || !Directory.Exists(target))
    {
        return Results.BadRequest("Invalid path");
    }

    var extensions = s.GetExtensionSet();
    var enumOpts = new EnumerationOptions
    {
        RecurseSubdirectories = req.Recursive,
        IgnoreInaccessible = true,
    };

    var files = Directory.EnumerateFiles(target, "*", enumOpts)
        .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
        .Where(f => !File.Exists(Path.ChangeExtension(f, ".mp3")))
        .ToList();

    int queued = files.Count(f => tracker.Enqueue(f, req.KeepOriginals));

    return Results.Ok(new { queued, skipped = files.Count - queued });
});

// ── job list (polled every 2 s by the frontend) ───────────────────────────────

app.MapGet("/api/jobs", (ConversionTracker tracker) =>
{
    var jobs = tracker.GetAll().Select(j => new
    {
        path    = j.Path,
        name    = Path.GetFileName(j.Path),
        dir     = Path.GetDirectoryName(j.Path) ?? "",
        status  = j.Status.ToString().ToLowerInvariant(),
        elapsed = j.ElapsedSeconds,
        error   = j.Error,
    });

    var (q, cv, d, e) = tracker.GetStats();
    return Results.Ok(new { stats = new { queued = q, converting = cv, done = d, errors = e }, jobs });
});

// ── clear completed / failed jobs ─────────────────────────────────────────────

app.MapDelete("/api/jobs", (ConversionTracker tracker) =>
    Results.Ok(new { cleared = tracker.ClearDone() }));

app.Run();

// ── helpers ───────────────────────────────────────────────────────────────────

static string NormalisePath(string path) =>
    Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);

static bool IsWithinRoot(string path, string root) =>
    path.Equals(root, StringComparison.OrdinalIgnoreCase) ||
    path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

static object[] BuildBreadcrumbs(string path, string root)
{
    var crumbs = new List<object>();
    var rootName = Path.GetFileName(root) is { Length: > 0 } n ? n : root;
    crumbs.Add(new { name = rootName, path = root });

    if (path.Length > root.Length)
    {
        var relative = path[(root.Length + 1)..];
        var parts = relative.Split(Path.DirectorySeparatorChar);
        var current = root;

        foreach (var part in parts)
        {
            current = Path.Combine(current, part);
            crumbs.Add(new { name = part, path = current });
        }
    }

    return crumbs.ToArray();
}

static string[] SafeGetDirectories(string path)
{
    try
    {
        return Directory.GetDirectories(path);
    }
    catch
    {
        return [];
    }
}

static string[] SafeGetFiles(string path)
{
    try
    {
        return Directory.GetFiles(path);
    }
    catch
    {
        return [];
    }
}

record ConvertRequest(string Path, bool KeepOriginals = false, bool Recursive = true);
