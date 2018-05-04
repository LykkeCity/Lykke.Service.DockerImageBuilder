using Lykke.Service.DockerImageBuilder.Core.Domain;
using Lykke.Service.DockerImageBuilder.Core.Services;
using System;
using System.Collections.Generic;

namespace Lykke.Service.DockerImageBuilder.Services
{
    public class DockerHubInfoProvider : IDockerHubInfoProvider
    {
        private readonly Dictionary<DockerHubType, string> _dockerHubNames;

        public DockerHubInfoProvider(Dictionary<DockerHubType, string> dockerHubNames)
        {
            _dockerHubNames = dockerHubNames;
        }

        public string GetDockerHubName(DockerHubType dockerHubType)
        {
            if (!_dockerHubNames.ContainsKey(dockerHubType))
                throw new InvalidOperationException($"Unknown docker hub type {dockerHubType}");

            return _dockerHubNames[dockerHubType];
        }

        public DockerHubType GetHubTypeFromImageFullName(string fullDockerImageName)
        {
            foreach (var dockerHubInfo in _dockerHubNames)
            {
                if (fullDockerImageName.StartsWith(dockerHubInfo.Value))
                    return dockerHubInfo.Key;
            }
            throw new InvalidOperationException($"Can't determine docker hub from full docker image name");
        }
    }
}
