using System.Threading.Tasks;

namespace Lykke.Service.DockerImageBuilder.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
