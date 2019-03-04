using FortniteReplayUploader;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Caching;
using System.ServiceProcess;

namespace FortniteReplayWatcher
{

    public class FortniteReplayWatcher : ServiceBase
    {
        private FileSystemWatcher _watcher;
        private Settings _settings;

        private UploadService _uploadService;
        private ParseService _parseService;

        private MemoryCache _memCache;
        private CacheItemPolicy _cacheItemPolicy;

        //private const int CacheTimeMilliseconds = 3000; // 3 seconds
        private const int CacheTimeMilliseconds = 600000; // 10 minutes

        protected override void OnStart(string[] args)
        {
            _settings = ReadJsonFile();
            if (_settings.Username == null)
            {
                Logger("No username provided...");
                throw new NoUsernameException();
            }
            _uploadService = new UploadService(_settings.Endpoint);
            _parseService = new ParseService();

            _memCache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy()
            {
                //RemovedCallback = OnRemovedFromCache
            };

            
            _watcher = new FileSystemWatcher(_settings.Path, _settings.FileExtension)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.EnableRaisingEvents = true;
            Logger("Started watching...");
        }

        protected override void OnStop()
        {
            _watcher.EnableRaisingEvents = false;
            _memCache.Dispose();
            _watcher.Dispose();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            _cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);

            string msg = $"Parsing file {e.FullPath} {e.ChangeType}";

            var cache_offset = _memCache.Get($"offset_{e.Name}");
            var offset = cache_offset == null ? 0 : Convert.ToInt32(cache_offset);
            var new_offset = _parseService.Parse(_settings.Username, e.FullPath, offset);
            new_offset = new_offset == null ? 0 : new_offset;
            _memCache.Set($"offset_{e.Name}", new_offset, _cacheItemPolicy);

            _memCache.Set(e.Name, e, _cacheItemPolicy);
        }
        
        //private void OnRemovedFromCache2(CacheEntryRemovedArguments args)
        //{
        //    if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;
        //    var e = (FileSystemEventArgs)args.CacheItem.Value;

        //    string msg = $"Parsing file {e.FullPath} {e.ChangeType}";
        //    Logger(msg);

        //    try
        //    {
        //        var cache_offset = _memCache.Get($"offset_{e.Name}");
        //        var offset = cache_offset == null ? 0 : Convert.ToInt32(cache_offset);
        //        var new_offset = _parseService.Parse(_settings.Username, e.FullPath, offset);
        //        new_offset = new_offset == null ? 0 : new_offset;
        //        _memCache.Set($"offset_{e.Name}", new_offset, DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds2));
                
        //        Logger($"Parse completed... {new_offset}");
        //    }
        //    catch (UploadException ex)
        //    {
        //        Logger(ex.Message, EventLogEntryType.Error);
        //    }
        //}


        // Handle cache item expiring
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;

            // Now actually handle file event
            var e = (FileSystemEventArgs)args.CacheItem.Value;

            string msg = $"Uploading file {e.FullPath} {e.ChangeType}";
            Logger(msg);

            try
            {
                var response = _uploadService.Upload(_settings.Username, e.FullPath).Result;
                Logger(response);
                Logger("Upload completed");
            }
            catch (UploadException ex)
            {
                Logger(ex.Message, EventLogEntryType.Error);
            }
        }

        private Settings ReadJsonFile()
        {
            var path = Path.Combine(new string[] { Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FortniteReplayWatcher", "settings.json" });

            Settings settings;
            if (File.Exists(path))
            {
                Logger("Config File exists");

                try
                {
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
                }
                catch (Exception e)
                {
                    Logger("couldnt load config file.... => " + e.Message, EventLogEntryType.Error);
                    settings = new Settings();
                }
            }
            else
            {
                Logger("config file doesnt exists", EventLogEntryType.Error);
                settings = new Settings();
            }

            return settings;
        }

        private void Logger(string msg)
        {
            Logger(msg, EventLogEntryType.Information);
        }

        private void Logger(string msg, EventLogEntryType type)
        {
            DateTime dt = new DateTime();
            dt = System.DateTime.UtcNow;
            msg = dt.ToLocalTime() + ": " + msg;
            EventLog.WriteEntry("FortniteReplayWatcher", msg, type);
        }
    }

    class NoUsernameException : Exception
    {

    }
}
