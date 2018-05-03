using Lykke.Service.DockerImageBuilder.Core.Domain;

namespace Lykke.Service.DockerImageBuilder.Core.Services
{
    public interface IDockerHubInfoProvider
    {
        (string,string) GetDockerHubInfo(DockerHubType dockerHubType);

        DockerHubType GetHubTypeFromImageFullName(string fullDockerImageName);
    }
}
