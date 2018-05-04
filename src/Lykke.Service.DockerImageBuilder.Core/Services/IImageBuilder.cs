namespace Lykke.Service.DockerImageBuilder.Core.Services
{
    public interface IImageBuilder
    {
        string BuildDirectory { get; }

        void FetchSources(string commitId);

        void BuildAndPublishApp(string buildNumber);

        void BuildDockerImage(string fullImageName);

        void PublishDocker(string fullImageName);
    }
}
