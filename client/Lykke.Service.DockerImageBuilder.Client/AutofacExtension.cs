using System;
using Autofac;
using Common.Log;

namespace Lykke.Service.DockerImageBuilder.Client
{
    public static class AutofacExtension
    {
        public static void RegisterDockerImageBuilderClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<DockerImageBuilderClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IDockerImageBuilderClient>()
                .SingleInstance();
        }

        public static void RegisterDockerImageBuilderClient(this ContainerBuilder builder, DockerImageBuilderServiceClientSettings settings, ILog log)
        {
            builder.RegisterDockerImageBuilderClient(settings?.ServiceUrl, log);
        }
    }
}
