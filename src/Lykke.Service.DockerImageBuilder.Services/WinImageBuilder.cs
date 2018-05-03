using Common.Log;
using Lykke.Service.DockerImageBuilder.Core.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Lykke.Service.DockerImageBuilder.Services
{
    public class WinImageBuilder : IImageBuilder
    {
        private const string _gitSuffix = ".git";
        private const string _publishPath = "app/dist";

        private readonly string _diskPath;
        private readonly string _baseWinImage;
        private readonly string _gitRepoUrl;
        private readonly string _workingPath;
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
        }

        public void FetchSources(string commitId)
        {
            ExecuteCommand("git", "init");
            ExecuteCommand("git", $"remote add origin {_gitRepoUrl}");
            ExecuteCommand("git", $"fetch origin {commitId}");
            ExecuteCommand("git", $"reset --hard FETCH_HEAD");
        }

        public void BuildAndPublishApp()
        {
            var slnFiles = Directory.EnumerateFiles(_workingPath, "*.sln");
            if (!slnFiles.Any())
                throw new InvalidOperationException($"Coildn't find any *.sln file from {_gitRepoUrl} on path {_workingPath}");
            string slnFile = slnFiles.First();
            ExecuteCommand("dotnet", $"build {slnFile} /p:Configuration=Release /p:Platform=\"Any CPU\"");
            ExecuteCommand("dotnet", $"publish {slnFile} /p:Configuration=Release /p:Platform=\"Any CPU\" --no-restore --output {_publishPath}");
        }

        public void BuildDockerImage(string fullImageName)
        {
            ExecuteCommand("docker ", $"build -t {fullImageName} {_publishPath}");
        }

        public void PublishDocker(string fullImageName)
        {
            var dockerHubType = _dockerHubInfoProvider.GetHubTypeFromImageFullName(fullImageName);
            (string dockerHubName, string dockerHubPass) = _dockerHubInfoProvider.GetDockerHubInfo(dockerHubType);
            ExecuteCommand("docker ", $"login -u {dockerHubName} -p {dockerHubPass}");
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
