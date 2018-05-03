using System;
using Common.Log;

namespace Lykke.Service.DockerImageBuilder.Client
{
    public class DockerImageBuilderClient : IDockerImageBuilderClient, IDisposable
    {
        private readonly ILog _log;

        public DockerImageBuilderClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
