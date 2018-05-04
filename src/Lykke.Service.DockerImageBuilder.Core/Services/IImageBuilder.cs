namespace Lykke.Service.DockerImageBuilder.Core.Services
{
    public interface IImageBuilder
    {
        string BuildDirectory { get; }

        void FetchSources(string commitId);

        void BuildAndPublishApp();

        void BuildDockerImage(string fullImageName);

        void PushToDockerHub(string fullImageName, string dockerHubPassword);
    }
}
