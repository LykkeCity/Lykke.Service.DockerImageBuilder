using Lykke.Service.DockerImageBuilder.Core.Domain;
using Lykke.Service.DockerImageBuilder.Core.Services;
using System;
using System.Collections.Generic;

namespace Lykke.Service.DockerImageBuilder.Services
{
    public class DockerHubInfoProvider : IDockerHubInfoProvider
    {
        private readonly Dictionary<DockerHubType, string> _dockerHubNames;
        private readonly Dictionary<DockerHubType, string> _dockerHubPasswords;

        public DockerHubInfoProvider(
            Dictionary<DockerHubType, string> dockerHubNames,
            Dictionary<DockerHubType, string> dockerHubPasswords)
        {
            _dockerHubNames = dockerHubNames;
            _dockerHubPasswords = dockerHubPasswords;
        }

        public (string, string) GetDockerHubInfo(DockerHubType dockerHubType)
        {
            if (!_dockerHubNames.ContainsKey(dockerHubType))
                throw new InvalidOperationException($"Unknown docker hub type {dockerHubType}");

            if (!_dockerHubPasswords.ContainsKey(dockerHubType))
                throw new InvalidOperationException($"Don't know a password for hub type {dockerHubType}");

            return (_dockerHubNames[dockerHubType], _dockerHubPasswords[dockerHubType]);
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
