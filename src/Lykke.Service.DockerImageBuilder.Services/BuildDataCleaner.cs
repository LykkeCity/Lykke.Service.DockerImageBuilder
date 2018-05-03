using Common;
using Common.Log;
using Lykke.Service.DockerImageBuilder.Core.Services;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Lykke.Service.DockerImageBuilder.Services
{
    public class BuildDataCleaner : TimerPeriod, IBuildDataCleaner
    {
        private readonly ILog _log;
        private readonly ConcurrentQueue<string> _cleanupQueue = new ConcurrentQueue<string>();

        public BuildDataCleaner(ILog log)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, log)
        {
            _log = log;
            DisableTelemetry();
            Start();
        }

        public void CleanUp(string path)
        {
            _cleanupQueue.Enqueue(path);
        }

        public override Task Execute()
        {
            while (_cleanupQueue.TryDequeue(out string path))
            {
                DeleteDirectory(path);
            }
            return Task.CompletedTask;
        }

        private void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            var files = Directory.EnumerateFiles(path);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
            var subdirs = Directory.EnumerateDirectories(path);
            foreach (var subdir in subdirs)
            {
                DeleteDirectory(subdir);
            }
            try
            {
                Directory.Delete(path);
            }
            catch
            {
            }
        }
    }
}
