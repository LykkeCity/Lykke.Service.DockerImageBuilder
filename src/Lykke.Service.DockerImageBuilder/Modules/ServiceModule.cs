using Autofac;
using Common.Log;
using Lykke.Service.DockerImageBuilder.Core.Domain;
using Lykke.Service.DockerImageBuilder.Core.Services;
using Lykke.Service.DockerImageBuilder.Settings;
using Lykke.Service.DockerImageBuilder.Services;
using System;
using System.Collections.Generic;

namespace Lykke.Service.DockerImageBuilder.Modules
{
    public class ServiceModule : Module
    {
        private readonly DockerImageBuilderSettings _settings;
        private readonly string _devDockerHubPassword;
        private readonly string _prodDockerHubPassword;
        private readonly ILog _log;

        public ServiceModule(
            DockerImageBuilderSettings settings,
            string devDockerHubPassword,
            string prodDockerHubPassword,
            ILog log)
        {
            _settings = settings;
            _devDockerHubPassword = devDockerHubPassword;
            _prodDockerHubPassword = prodDockerHubPassword;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            var dockerHubNamesDict = new Dictionary<DockerHubType, string>();
            foreach (var dockerHubInfo in _settings.DockerHubs)
            {
                var hubType = (DockerHubType)Enum.Parse(typeof(DockerHubType), dockerHubInfo.HubType);
                dockerHubNamesDict.Add(hubType, dockerHubInfo.HubName);
            }

            var dockerHubPasswordsDict = new Dictionary<DockerHubType, string>()
            {
                { DockerHubType.Dev, _devDockerHubPassword },
                { DockerHubType.Prod, _prodDockerHubPassword },
            };

            builder.RegisterType<DockerHubInfoProvider>()
                .As<IDockerHubInfoProvider>()
                .WithParameter("dockerHubNames", dockerHubNamesDict)
                .WithParameter("dockerHubPasswords", dockerHubPasswordsDict);

            builder.RegisterType<ImageBuilderFactory>()
                .As<IImageBuilderFactory>()
                .WithParameter("diskPath", _settings.DiskPath)
                .WithParameter("baseWinImage", _settings.BaseWindowsImage);

            builder.RegisterType<BuildDataCleaner>()
                .As<IBuildDataCleaner>()
                .SingleInstance();
        }
    }
}
