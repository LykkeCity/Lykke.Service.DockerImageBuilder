using Common.Log;
using Lykke.Service.DockerImageBuilder.Core.Services;
using System;
using System.IO;

namespace Lykke.Service.DockerImageBuilder.Services
{
    public class ImageBuilderFactory : IImageBuilderFactory
    {
        private readonly string _diskPath;
        private readonly string _baseWinImage;
        private readonly IDockerHubInfoProvider _dockerHubInfoProvider;
        private readonly ILog _log;

        public ImageBuilderFactory(
            string diskPath,
            string baseWinImage,
            IDockerHubInfoProvider dockerHubInfoProvider,
            ILog log)
        {
            _diskPath = diskPath;
            _baseWinImage = baseWinImage;
            _dockerHubInfoProvider = dockerHubInfoProvider;
            _log = log;

            if (!Directory.Exists(_diskPath))
                Directory.CreateDirectory(_diskPath);
            Environment.CurrentDirectory = _diskPath;
        }

        public IImageBuilder CreateWinImageBuilder(string gitRepoUrl)
        {
            return new WinImageBuilder(
                _diskPath,
                _baseWinImage,
                gitRepoUrl,
                _dockerHubInfoProvider,
                _log);
        }
    }
}
