using System;
using System.Collections.Generic;

namespace FortniteReplayWatcher
{
    public class Settings
    {
        public string Username { get; set; }
        public string Path { get; set; }
        public string FileExtension { get; set; } = "*.replay";
        public string Endpoint { get; set; } = "https://fortnite-replay-api.herokuapp.com/replays/upload";
    }
}
