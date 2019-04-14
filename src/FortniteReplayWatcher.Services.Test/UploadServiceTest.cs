using System.Threading.Tasks;
using Xunit;

namespace FortniteReplayWatcher.Services.Test
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var service = new UploadService("http://localhost:5000/replays/upload/");
            var content = await service.Upload("Shiqan", @"Replays\UnsavedReplay-2018.10.17-20.22.26.replay");
            Assert.Contains("success", content);
        }
    }
}
