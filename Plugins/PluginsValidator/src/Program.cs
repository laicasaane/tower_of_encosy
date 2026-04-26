// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Csv;

var argSpan = args.AsSpan();
var argLength = argSpan.Length;
var rootDirectory = string.Empty;

for (var i = 0; i < argLength; i++)
{
    var id = argSpan[i];

    if (i + 1 >= argLength)
    {
        break;
    }

    var arg = argSpan[i + 1] ?? string.Empty;

    if (arg.StartsWith('-'))
    {
        continue;
    }

    switch (id)
    {
        case "--root-directory":
            rootDirectory = arg;
            break;
    }
}

if (string.IsNullOrEmpty(rootDirectory))
{
    using (Console.WithColor(ConsoleColor.Red))
    {
        Console.WriteLine("Root directory is not specified. Use '--root-directory <path>' to specify it.");
    }

    return 1;
}

if (Directory.Exists(rootDirectory) == false)
{
    using (Console.WithColor(ConsoleColor.Red))
    {
        Console.WriteLine($"Root directory '{rootDirectory}' does not exist.");
    }

    return 1;
}

using (Console.WithColor(ConsoleColor.Cyan))
{
    Console.WriteLine($"Root directory: '{rootDirectory}'");
}

var pluginsFilePath = Path.Combine(rootDirectory, "Plugins.csv");
var srcReposRootPath = Path.Combine(rootDirectory, "SrcRepos");
var unityPluginsRootPath = Path.Combine(rootDirectory, "UnityPlugins");

if (Directory.Exists(srcReposRootPath) == false)
{
    Directory.CreateDirectory(srcReposRootPath);

    var gitignoreFilePath = Path.Combine(srcReposRootPath, ".gitignore");
    File.WriteAllText(gitignoreFilePath, "**/\n", Encoding.UTF8);
}

if (Directory.Exists(unityPluginsRootPath) == false)
{
    Directory.CreateDirectory(unityPluginsRootPath);

    var gitignoreFilePath = Path.Combine(unityPluginsRootPath, ".gitignore");
    File.WriteAllText(gitignoreFilePath,
        """
        [Bb]in/
        [Oo]bj/

        !**/*.csproj
        !**/*.sln
        """
    );
}

if (File.Exists(pluginsFilePath) == false)
{
    File.WriteAllText(pluginsFilePath, "git_url,folder_within_src_repos");
}

var csv = File.ReadAllText(pluginsFilePath);
var gitFolders = new List<GitInfo>();

var csvOptions = new CsvOptions {
    ValidateColumnCount = false,
    ReturnEmptyForMissingColumn = true,
    AllowBackSlashToEscapeQuote = true,
    NewLine = "\n",
};

const string GIT_URL = "git_url";
const string FOLDER_WITHIN_SRC_REPOS = "folder_within_src_repos";
const string COMMIT_SHA = "commit_sha";

using (Console.WithColor(ConsoleColor.Green))
{
    Console.WriteLine($"Processing '{pluginsFilePath}'");
}

foreach (var line in CsvReader.ReadFromText(csv, csvOptions))
{
    if (line == null)
    {
        continue;
    }

    if (line.HasColumn(GIT_URL) == false)
    {
        continue;
    }

    var gitUrl = line[GIT_URL];

    if (string.IsNullOrWhiteSpace(gitUrl))
    {
        continue;
    }

    var folderName = string.Empty;

    if (line.HasColumn(FOLDER_WITHIN_SRC_REPOS))
    {
        try
        {
            folderName = line[FOLDER_WITHIN_SRC_REPOS];
        }
        catch
        {
        }
    }

    var commitSha = string.Empty;

    if (line.HasColumn(COMMIT_SHA))
    {
        try
        {
            commitSha = line[COMMIT_SHA];
        }
        catch
        {
        }
    }

    if (string.IsNullOrWhiteSpace(folderName))
    {
        var segments = gitUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var repoName = segments.LastOrDefault();

        if (string.IsNullOrEmpty(repoName) == false
            && repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
        )
        {
            repoName = repoName[..^4];
        }

        if (string.IsNullOrEmpty(repoName))
        {
            using (Console.WithColor(ConsoleColor.Yellow))
            {
                Console.WriteLine($"Cannot extract repository name from '{gitUrl}'");
            }

            continue;
        }

        if (segments.Length < 2)
        {
            using (Console.WithColor(ConsoleColor.Yellow))
            {
                Console.WriteLine($"Cannot extract author name from '{gitUrl}'");
            }

            continue;
        }

        var authorName = segments[^2];

        folderName = $"{authorName}.{repoName}";
    }

    gitFolders.Add(new(gitUrl, folderName, commitSha));
}

if (gitFolders.Count < 1)
{
    using (Console.WithColor(ConsoleColor.Red))
    {
        Console.WriteLine($"No Git URL found in '{pluginsFilePath}'");
    }

    return 1;
}

var processingGitFolders = new List<GitInfo>();

foreach (var (gitUrl, folderName, commitSha) in gitFolders)
{
    var srcRepoPath = Path.Combine(srcReposRootPath, folderName);

    if (Directory.Exists(srcRepoPath))
    {
        try
        {
            Directory.Delete(srcRepoPath, true);
        }
        catch (UnauthorizedAccessException)
        {
            try
            {
                var allFiles = Directory.GetFiles(srcRepoPath, "*", SearchOption.AllDirectories);

                foreach (var filePath in allFiles)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                Directory.Delete(srcRepoPath, true);
            }
            catch (Exception ex)
            {
                using (Console.WithColor(ConsoleColor.Red))
                {
                    Console.WriteLine($"Cannot delete '{srcRepoPath}'");
                    Console.WriteLine(ex);
                }

                continue;
            }
        }
        catch (Exception ex)
        {
            using (Console.WithColor(ConsoleColor.Red))
            {
                Console.WriteLine($"Cannot delete '{srcRepoPath}'");
                Console.WriteLine(ex);
            }

            continue;
        }
    }

    processingGitFolders.Add(new(gitUrl, folderName, commitSha));
}

foreach (var info in processingGitFolders)
{
    var cloneResult = CloneGitRepo(info, srcReposRootPath);

    if (cloneResult && string.IsNullOrEmpty(info.CommitSha) == false)
    {
        ResetToCommitSha(info, srcReposRootPath);
    }
}

static bool CloneGitRepo(GitInfo info, string srcReposRootPath)
{
    var (gitUrl, folderName, _) = info;
    var srcRepoPath = Path.Combine(srcReposRootPath, folderName);

    using (Console.WithColor(ConsoleColor.Green))
    {
        Console.WriteLine($"Cloning '{gitUrl}' into '{srcRepoPath}'");
    }

    using var gitProcess = new Process {
        StartInfo = new ProcessStartInfo {
            FileName = "git",
            Arguments = $"clone \"{gitUrl}\" \"{srcRepoPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        }
    };

    try
    {
        gitProcess.Start();

        var output = gitProcess.StandardOutput.ReadToEnd();
        var error = gitProcess.StandardError.ReadToEnd();
        gitProcess.WaitForExit();

        if (gitProcess.ExitCode != 0)
        {
            using (Console.WithColor(ConsoleColor.Red))
            {
                Console.WriteLine($"Git clone failed for '{gitUrl}' with exit code {gitProcess.ExitCode}");
                Console.WriteLine(error);
            }
            return false;
        }

        using (Console.WithColor(ConsoleColor.Cyan))
        {
            Console.WriteLine(output);
        }

        return true;
    }
    catch (Exception ex)
    {
        using (Console.WithColor(ConsoleColor.Red))
        {
            Console.WriteLine($"Exception occurred while cloning '{gitUrl}': {ex.Message}");
        }

        return false;
    }
}

static void ResetToCommitSha(GitInfo info, string srcReposRootPath)
{
    var (gitUrl, folderName, commitSha) = info;
    var srcRepoPath = Path.Combine(srcReposRootPath, folderName);

    using (Console.WithColor(ConsoleColor.Green))
    {
        Console.WriteLine($"Resetting '{srcRepoPath}' to commit '{commitSha}'");
    }

    using var gitProcess = new Process {
        StartInfo = new ProcessStartInfo {
            FileName = "git",
            Arguments = $"reset --hard {commitSha}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = srcRepoPath,
        }
    };

    try
    {
        gitProcess.Start();

        var output = gitProcess.StandardOutput.ReadToEnd();
        var error = gitProcess.StandardError.ReadToEnd();
        gitProcess.WaitForExit();

        if (gitProcess.ExitCode != 0)
        {
            using (Console.WithColor(ConsoleColor.Red))
            {
                Console.WriteLine($"Git reset failed for '{gitUrl}' with exit code {gitProcess.ExitCode}");
                Console.WriteLine(error);
            }
        }
        else
        {
            using (Console.WithColor(ConsoleColor.Cyan))
            {
                Console.WriteLine(output);
            }
        }
    }
    catch (Exception ex)
    {
        using (Console.WithColor(ConsoleColor.Red))
        {
            Console.WriteLine($"Exception occurred while resetting '{gitUrl}': {ex.Message}");
        }
    }
}

var unityPluginsSlnxPath = Path.Combine(unityPluginsRootPath, "Plugins.slnx");

if (File.Exists(unityPluginsSlnxPath) == false)
{
    File.WriteAllText(unityPluginsSlnxPath,
        """
        <Solution>
        </Solution>
        """
    );
}

PluginsIntegrator.Slnx.Solution? slnx;

using (var reader = new StreamReader(unityPluginsSlnxPath))
{
    try
    {
        var serializer = new XmlSerializer(typeof(PluginsIntegrator.Slnx.Solution));
        slnx = (PluginsIntegrator.Slnx.Solution?)serializer.Deserialize(reader);
    }
    catch (Exception ex)
    {
        using (Console.WithColor(ConsoleColor.Red))
        {
            Console.WriteLine($"Failed to deserialize '{unityPluginsSlnxPath}': {ex.Message}");
        }

        return 1;
    }
}

if (slnx == null)
{
    using (Console.WithColor(ConsoleColor.Red))
    {
        Console.WriteLine($"Failed to deserialize '{unityPluginsSlnxPath}'");
    }

    return 1;
}

foreach (var (gitUrl, folderName, commitSha) in processingGitFolders)
{
    var unityPluginPath = Path.Combine(unityPluginsRootPath, folderName);

    if (Directory.Exists(unityPluginPath) == false)
    {
        Directory.CreateDirectory(unityPluginPath);
    }

    var gitUrlWithoutSuffix = gitUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
        ? gitUrl[..^4]
        : gitUrl;

    var repositoryUrl = string.IsNullOrWhiteSpace(commitSha)
        ? gitUrl
        : $"{gitUrlWithoutSuffix}/tree/{commitSha}";

    var csprojFilePath = GetCsprojFilePath(unityPluginsRootPath, folderName, out var srcFolderName);

    if (File.Exists(csprojFilePath) == false)
    {
        File.WriteAllText(csprojFilePath,
            $"""
            <Project Sdk="Microsoft.NET.Sdk">

                <PropertyGroup>
                    <RepositoryUrl>{repositoryUrl}</RepositoryUrl>
                    <TargetFrameworks>netstandard2.1</TargetFrameworks>
                    <SrcRepoDir>../../SrcRepos/{folderName}/</SrcRepoDir>
                </PropertyGroup>

                <!--<ItemGroup>
                    <Compile Include="$(SrcRepoDir)/**/*.cs"
                        Exclude="$(SrcRepoDir)/obj/**/*.*" />
                </ItemGroup>-->

            </Project>
            """
        );
    }

    var csprojFilePathRelative = (string.IsNullOrEmpty(srcFolderName)
        ? Path.Combine(folderName, $"{folderName}.csproj")
        : Path.Combine(folderName, srcFolderName, $"{folderName}.csproj")
    ).Replace('\\', '/');

    var index = slnx.Projects.FindIndex(
        x => string.Equals(x.Path, csprojFilePathRelative, StringComparison.InvariantCultureIgnoreCase)
    );

    if (index < 0)
    {
        slnx.Projects.Add(new PluginsIntegrator.Slnx.Project {
            Path = csprojFilePathRelative,
        });
    }

    using (Console.WithColor(ConsoleColor.Cyan))
    {
        Console.WriteLine($"Successfully verified project '{csprojFilePathRelative}'");
    }
}

static string GetCsprojFilePath(string unityPluginsRootPath, string folderName, out string srcFolderName)
{
    var csprojFilePathOutsideSrc = Path.Combine(unityPluginsRootPath, folderName, $"{folderName}.csproj");
    var csprojFilePathWithinLowerCaseSrc = Path.Combine(unityPluginsRootPath, folderName, "src", $"{folderName}.csproj");
    var csprojFilePathWithinUpperCaseSrc = Path.Combine(unityPluginsRootPath, folderName, "Src", $"{folderName}.csproj");

    if (File.Exists(csprojFilePathWithinUpperCaseSrc))
    {
        srcFolderName = "Src";
        return csprojFilePathWithinUpperCaseSrc;
    }

    if (File.Exists(csprojFilePathWithinLowerCaseSrc))
    {
        srcFolderName = "src";
        return csprojFilePathWithinLowerCaseSrc;
    }

    srcFolderName = string.Empty;
    return csprojFilePathOutsideSrc;
}

var settings = new XmlWriterSettings {
    Indent = true,
    OmitXmlDeclaration = true,
    Encoding = Encoding.UTF8,
    NewLineChars = "\n",
};

var ns = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);

using (var stream = new StreamWriter(unityPluginsSlnxPath))
using (var writer = XmlWriter.Create(stream, settings))
{
    var serializer = new XmlSerializer(typeof(PluginsIntegrator.Slnx.Solution));
    serializer.Serialize(writer, slnx, ns);
}

return 0;
