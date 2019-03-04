using FortniteReplayObservers.File;
using FortniteReplayObservers.Mqtt;
using FortniteReplayReader;
using FortniteReplayReader.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;

namespace FortniteReplayUploader
{
    public class ParseService
    {
        private MemoryCache _memCache;
        private CacheItemPolicy _cacheItemPolicy;
        private const int CacheTimeMilliseconds = 600000;

        public ParseService()
        {
            _memCache = MemoryCache.Default;
            _cacheItemPolicy = new CacheItemPolicy();
        }

        public long? Parse(string username, string path, int offset)
        {
            _cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(CacheTimeMilliseconds);

            var cacheObject = _memCache.Get("parseService");
            var cache = cacheObject == null ? new Dictionary<PlayerElimination, int>() : (Dictionary<PlayerElimination, int>)cacheObject;
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new ElimObservableFortniteBinaryReader(stream, offset))
                {
                    //var mqttObserver = new MqttObserver(cache);
                    //var fileObserver = new FileObserver(cache);

                    //mqttObserver.Subscribe(reader);
                    //fileObserver.Subscribe(reader);

                    var replay = reader.ReadFile();
                    var addEliminationsToCache = replay.Eliminations.Concat(cache.Keys).ToDictionary(k => k, v => 1);

                    _memCache.Set("parseService", addEliminationsToCache, _cacheItemPolicy);
                    return stream.Length;
                }
            }
        }
    }
}
