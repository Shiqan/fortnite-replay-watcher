using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FortniteReplayWatcher.Services
{
    public class UploadService
    {
        private readonly HttpClient _client;
        private readonly string _endpoint;

        public UploadService(string endpoint)
        {
            _client = new HttpClient();
            _endpoint = endpoint;
        }

        public async Task<string> Upload(string username, string path)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(new StringContent(username), "username");

                    var fileContent = new StreamContent(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    formData.Add(fileContent, "data_file", "data_file");

                    var response = await _client.PostAsync(_endpoint, formData);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new UploadException($"Failed to upload because {ex.Message}");
            }
        }
    }

    public class UploadException : Exception
    {
        public UploadException(string message) : base(message) { }

    }
}
