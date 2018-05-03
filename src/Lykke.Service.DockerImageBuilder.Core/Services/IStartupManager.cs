using System.Threading.Tasks;

namespace Lykke.Service.DockerImageBuilder.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}