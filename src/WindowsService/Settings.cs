using System;

namespace FortniteReplayWatcher
{
    public class Settings
    {
        public string Username { get; set; }
        public string Path { get; set; } = System.IO.Path.Combine(new string[] { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FortniteGame", "Saved", "Demos" });
        public string FileExtension { get; set; } = "*.replay";
        public string Endpoint { get; set; } = "http://fortnite-replay-api.herokuapp.com/replay/upload";
    }
}
