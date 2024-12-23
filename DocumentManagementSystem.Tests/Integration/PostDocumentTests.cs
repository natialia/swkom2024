﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace DocumentManagementSystem.Tests.Integration
{
    [Collection("IntegrationTests")]
    public class PostDocumentTests : IAsyncLifetime
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string DockerComposeFilePath = "./../../../../docker-compose-test.yml";

        public async Task InitializeAsync()
        {
            //Start all necessary containers
            await DockerComposeHelper.StartDockerCompose(DockerComposeFilePath);
            //Wait until all services ready
            await WaitForServicesToBeReady();
        }

        public async Task DisposeAsync()
        {
            //Stop all running containers
            await DockerComposeHelper.StopDockerCompose(DockerComposeFilePath);
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
        public async Task PostDocument_UploadsFileSuccessfully_Returns201()
        {
            //Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration/Vocabulary.pdf");
            var fileContentBytes = await File.ReadAllBytesAsync(filePath); //read content of pdf

            // make bytearray content out of file
            var fileContent = new ByteArrayContent(fileContentBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            // Create MultipartFormDataContent with file, other properties
            var multipartContent = new MultipartFormDataContent
                {
                    // Add file
                    { fileContent, "UploadedDocument", "Vocabulary.pdf" },

                    //Document properties
                    { new StringContent("1"), "Id" },
                    { new StringContent("Vocabulary"), "Name" },
                    { new StringContent("application/pdf"), "FileType" },
                    { new StringContent("20kB"), "FileSize" }
                };

            // Act
            var response = await _httpClient.PostAsync("http://localhost:8081/Document", multipartContent);

            //Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("Vocabulary", responseBody);

            // ---- Get id of document from response ----
            var jsonResponse = JsonDocument.Parse(responseBody);
            var documentId = jsonResponse.RootElement.GetProperty("id").GetInt32();

            //Check if document persists
            var getResponse = await _httpClient.GetAsync($"http://localhost:8081/Document/{documentId}");
            // Assert GET response
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var getResponseBody = await getResponse.Content.ReadAsStringAsync();
            Assert.Contains("Vocabulary", getResponseBody);

        }
    }
}
