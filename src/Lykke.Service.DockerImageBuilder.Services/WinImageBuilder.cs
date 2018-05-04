using Common.Log;
using Lykke.Service.DockerImageBuilder.Core.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lykke.Service.DockerImageBuilder.Services
{
    public class WinImageBuilder : IImageBuilder
    {
        private const string _gitSuffix = ".git";
        private const string _publishLocalPath = "app\\dist";
        private const string _dockerfileName = "Dockerfile";
        private const string _imageLinePrefix = "FROM ";
        private const string _winImageSuffix = "-nano";
        private const string _slnFilesPattern = "*.sln";
        private const string _projFilesPattern = "*.csproj";
        private const string _versionLinePrefix = "<Version>";

        private readonly string _diskPath;
        private readonly string _baseWinImage;
        private readonly string _gitRepoUrl;
        private readonly string _workingPath;
        private readonly string _publishPath;
        private readonly IDockerHubInfoProvider _dockerHubInfoProvider;
        private readonly ILog _log;

        public string BuildDirectory { get { return _workingPath; } }

        public WinImageBuilder(
            string diskPath,
            string baseWinImage,
            string gitRepoUrl,
            IDockerHubInfoProvider dockerHubInfoProvider,
            ILog log)
        {
            _diskPath = diskPath;
            _baseWinImage = baseWinImage;
            _dockerHubInfoProvider = dockerHubInfoProvider;
            _log = log;

            var gitRepo = GetRepoFromUrl(gitRepoUrl);
            _gitRepoUrl = gitRepoUrl;
            if (!_gitRepoUrl.EndsWith(_gitSuffix))
                _gitRepoUrl = $"{_gitRepoUrl}{_gitSuffix}";
            _workingPath = Path.Combine(_diskPath, gitRepo, Guid.NewGuid().ToString());
            while (Directory.Exists(_workingPath))
            {
                _workingPath = Path.Combine(_diskPath, gitRepo, Guid.NewGuid().ToString());
            }
            Directory.CreateDirectory(_workingPath);
            _publishPath = Path.Combine(_workingPath, _publishLocalPath);
            Directory.CreateDirectory(_publishPath);
        }

        public void FetchSources(string commitId)
        {
            ExecuteCommand("git", "init");
            ExecuteCommand("git", $"remote add origin {_gitRepoUrl}");
            ExecuteCommand("git", $"fetch origin {commitId}");
            ExecuteCommand("git", $"reset --hard FETCH_HEAD");
        }

        public void BuildAndPublishApp(string buildNumber)
        {
            var slnFiles = Directory.EnumerateFiles(_workingPath, _slnFilesPattern);
            if (!slnFiles.Any())
                throw new InvalidOperationException($"Coildn't find any {_slnFilesPattern} file from {_gitRepoUrl}");
            string slnFile = slnFiles.First();
            var projFiles = Directory.EnumerateFiles(_workingPath, _projFilesPattern, SearchOption.AllDirectories);
            if (!projFiles.Any())
                throw new InvalidOperationException($"Coildn't find any {_projFilesPattern} file from {_gitRepoUrl}");
            foreach (var projFile in projFiles)
            {
                var lines = File.ReadAllLines(projFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains(_versionLinePrefix))
                        continue;
                    lines[i] = Regex.Replace(lines[i], ">[0-9.]+<", $">{buildNumber}<");
                    break;
                }
                File.WriteAllLines(projFile, lines);
            }
            ExecuteCommand("dotnet", $"build {slnFile} /p:Configuration=Release /p:Platform=\"Any CPU\"");
            ExecuteCommand("dotnet", $"publish {slnFile} /p:Configuration=Release /p:Platform=\"Any CPU\" --no-restore --output {_publishPath}");
        }

        public void BuildDockerImage(string fullImageName)
        {
            var dockerfiles = Directory.EnumerateFiles(_publishPath, _dockerfileName);
            if (!dockerfiles.Any())
                throw new InvalidOperationException("Couldn't locate any Dockerfile in solution publish directory");
            string dockerfile = dockerfiles.First();
            var lines = File.ReadAllLines(dockerfile);
            bool isImageReplaced = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith(_imageLinePrefix))
                    continue;
                lines[i] = $"{_imageLinePrefix}{_baseWinImage}";
                isImageReplaced = true;
                break;
            }
            if (!isImageReplaced)
                throw new InvalidOperationException("Couldn't locate base image in Dockerfile for replacing");
            File.WriteAllLines(dockerfile, lines);
            if (!fullImageName.EndsWith(_winImageSuffix))
                fullImageName = fullImageName + _winImageSuffix;
            ExecuteCommand("docker ", $"build -t {fullImageName} {_publishLocalPath}");
        }

        public void PublishDocker(string fullImageName)
        {
            var dockerHubType = _dockerHubInfoProvider.GetHubTypeFromImageFullName(fullImageName);
            (string dockerHubName, string dockerHubPass) = _dockerHubInfoProvider.GetDockerHubInfo(dockerHubType);
            ExecuteCommand("docker ", $"login -u {dockerHubName} -p {dockerHubPass}");
            if (!fullImageName.EndsWith(_winImageSuffix))
                fullImageName = fullImageName + _winImageSuffix;
            ExecuteCommand("docker ", $"push {fullImageName}");
            ExecuteCommand("docker ", $"rmi {fullImageName}");
        }

        private void ExecuteCommand(string processName, string arguments)
        {
            try
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = processName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = _workingPath,
                    }
                };
                process.Start();
                string processOutput = process.StandardOutput.ReadToEnd();
                string processErrors = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(processErrors) ? processOutput : processErrors);
                _log.WriteInfo(nameof(ExecuteCommand), new { processName, arguments }, processOutput);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(ExecuteCommand), new { processName, arguments }, ex);
                throw;
            }
        }

        private string GetRepoFromUrl(string url)
        {
            var parts = url.Split('\\', '/');
            var lastPart = parts[parts.Length - 1];
            if (lastPart.EndsWith(_gitSuffix))
            {
                int lastInd = lastPart.LastIndexOf('.');
                return lastPart.Substring(0, lastInd);
            }
            return lastPart;
        }
    }
}
