using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DocumentManagementSystem.Tests.Integration
{
    public static class DockerComposeHelper
    {
        public static async Task StartDockerCompose(string filePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = $"-f {filePath} up -d --build", //background start -d
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // avoid deadlocks and read seperately (output of process)
            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorOutput = await standardError;
                throw new Exception($"Failed to start docker-compose. Error: {errorOutput}");
            }

            await standardOutput;
        }

        public static async Task StopDockerCompose(string filePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = $"-f {filePath} down", // Stop container
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var standardOutput = process.StandardOutput.ReadToEndAsync();
            var standardError = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorOutput = await standardError;
                throw new Exception($"Failed to stop docker-compose. Error: {errorOutput}");
            }

            await standardOutput;
        }
    }
}
