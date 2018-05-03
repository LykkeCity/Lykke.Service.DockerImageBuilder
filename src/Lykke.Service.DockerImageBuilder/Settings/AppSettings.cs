using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.DockerImageBuilder.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public DockerImageBuilderSettings DockerImageBuilderService { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }

        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }

    public class SlackNotificationsSettings
    {
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }

    public class AzureQueuePublicationSettings
    {
        [AzureQueueCheck]
        public string ConnectionString { get; set; }

        public string QueueName { get; set; }
    }

    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive", false)]
        public string MonitoringServiceUrl { get; set; }
    }

    public class DockerImageBuilderSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public string DiskPath { get; set; }

        public string BaseWindowsImage { get; set; }

        public List<DockerHubSettings> DockerHubs { get; set; }
    }

    public class DockerHubSettings
    {
        public string HubType { get; set; }

        public string HubName { get; set; }
    }
}
