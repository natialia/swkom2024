using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace DocumentManagementSystem.Tests.Integration
{
    public class IntegrationTests : IAsyncLifetime
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string DockerComposeFilePath = "./../../../../docker-compose-test.yml";

        public async Task InitializeAsync()
        {
            //Start all necessary containers
            DockerComposeHelper.StartDockerCompose(DockerComposeFilePath);
            //Wait until all services ready
            await WaitForServicesToBeReady();
        }

        public async Task DisposeAsync()
        {
            //Stop all running containers
            //DockerComposeHelper.StopDockerCompose(DockerComposeFilePath);
        }

        private async Task WaitForServicesToBeReady()
        {
            var retries = 30;
            var delay = TimeSpan.FromSeconds(10);

            while (retries > 0)
            {
                try
                {
                    //Test if api reachable
                    var response = await _httpClient.GetAsync("http://localhost:8081/swagger/index.html");
                    if (response.IsSuccessStatusCode) return;
                }
                catch
                {
                    //Service not reachable, try again!!
                }

                retries--;
                await Task.Delay(delay);
            }

            throw new Exception($"Services not ready after {retries} tries.");
        }

        [Fact]
        public async Task PostDocument_UploadsFileSuccessfully_Returns201_AndStoresDocument()
        {
            // Arrange - create a pdf file
            var fileName = "testfile.pdf";
            var fileContentBytes = Encoding.UTF8.GetBytes("%PDF-1.4 Example PDF Content");
            var fileContent = new ByteArrayContent(fileContentBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            var multipartContent = new MultipartFormDataContent
                {
                    // create documentrequest-like object: add file
                    { fileContent, "UploadedDocument", fileName },
        
                    // add document properties
                    { new StringContent("1"), "Id" },
                    { new StringContent("Test Document"), "Name" },
                    { new StringContent("pdf"), "FileType" },
                    { new StringContent(fileContentBytes.Length.ToString()), "FileSize" }
                };

            // Act
            var response = await _httpClient.PostAsync("http://localhost:8081/Document", multipartContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("Test Document", responseBody);
        }
    }
}
