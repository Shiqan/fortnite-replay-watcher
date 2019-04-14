using FortniteReplayWatcher.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FortniteReplayWatcher.Worker
{
    public class Worker : BackgroundService
    {
        private FileSystemWatcher _watcher;
        private Settings _settings;

        private UploadService _uploadService;
        private ParseService _parseService;

        private IMemoryCache _memCache;
        private MemoryCacheEntryOptions _memCacheOptions;
        private const int CacheTimeMilliseconds = 300000; // 5 minutes
        private readonly ILogger<Worker> _logger;

        public Worker(IMemoryCache cache, ILogger<Worker> logger)
        {
            _memCache = cache;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Worker started at: {DateTime.Now}");

            _settings = ReadJsonFile();
            if (_settings.Username == null)
            {
                _logger.LogInformation("No username provided...");
                throw new Exception("No username provided");
            }
            _uploadService = new UploadService(_settings.Endpoint);
            _parseService = new ParseService();

            _memCache = new MemoryCache(new MemoryCacheOptions());
            _memCacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMilliseconds(CacheTimeMilliseconds))
                .RegisterPostEvictionCallback(callback: OnRemovedFromCache, state: this);

            _watcher = new FileSystemWatcher(_settings.Path, _settings.FileExtension)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.EnableRaisingEvents = true;
            _logger.LogInformation("Started watching...");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Worker stopped at: {DateTime.Now}");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTime.Now}");
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation($"Worker disposed at: {DateTime.Now}");
            base.Dispose();
        }


        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var msg = $"Parsing file {e.FullPath} {e.ChangeType}";

            var cache_offset = _memCache.Get($"offset_{e.Name}");
            var offset = cache_offset == null ? 0 : Convert.ToInt32(cache_offset);

            var new_offset = _parseService.Parse(_settings.Username, e.FullPath, offset);
            new_offset = new_offset == null ? 0 : new_offset;

            _memCache.Set($"offset_{e.Name}", new_offset, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds) });
            _memCache.Set(e.Name, e, _memCacheOptions);
        }

        // Handle cache item expiring
        private void OnRemovedFromCache(object key, object value, EvictionReason reason, object state)
        {
            if (reason != EvictionReason.Expired)
            {
                return;
            }

            // Now actually handle file event
            var e = (FileSystemEventArgs)value;

            var msg = $"Uploading file {e.FullPath} {e.ChangeType}";
            _logger.LogDebug(msg);

            try
            {
                var response = _uploadService.Upload(_settings.Username, e.FullPath).Result;
                _logger.LogDebug(response);
                _logger.LogDebug("Upload completed");
            }
            catch (UploadException ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private Settings ReadJsonFile()
        {
            var path = Path.Combine(new string[] { AppDomain.CurrentDomain.BaseDirectory, "settings.json" });

            Settings settings;
            if (File.Exists(path))
            {
                _logger.LogDebug("Config File exists");

                try
                {
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    _logger.LogError("couldnt load config file.... => " + e.Message);
                    settings = new Settings();
                }
            }
            else
            {
                _logger.LogError($"No config file found at {path}");
                settings = new Settings();
            }

            return settings;
        }
    }
}
