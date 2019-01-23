using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace FortniteReplayUploader.Tests
{
    [TestClass]
    public class FortniteReplayUploaderTests
    {
        [TestMethod]
        public async Task TestUploadSuccess()
        {
            var service = new UploadService("http://localhost:5000/replays/upload/");
            var content = await service.Upload("Shiqan", @"Replays\UnsavedReplay-2018.10.17-20.22.26.replay");
            Assert.IsTrue(content.Contains("success"));
        }
    }
}
