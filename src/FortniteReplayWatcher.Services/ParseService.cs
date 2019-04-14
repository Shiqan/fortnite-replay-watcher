using FortniteReplayReader;
using System.IO;

namespace FortniteReplayWatcher.Services
{
    public class ParseService
    {
        public long? Parse(string username, string path, int offset)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new ElimObservableFortniteBinaryReader(stream, offset))
                {
                    var replay = reader.ReadFile();
                    return stream.Length;
                }
            }
        }
    }
}
