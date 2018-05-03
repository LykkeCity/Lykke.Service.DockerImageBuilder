namespace Lykke.Service.DockerImageBuilder.Core.Services
{
    public interface IImageBuilderFactory
    {
        IImageBuilder CreateWinImageBuilder(string gitRepoUrl);
    }
}
