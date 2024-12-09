using ocr_worker.Workers;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ocr_worker.Services;
using Nest;

var worker = new OcrWorker();
worker.Start();

Console.WriteLine("OCR Worker is running. Press Ctrl+C to exit.");

while (true)
{
        Thread.Sleep(1000);
}

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IElasticClient>(new ElasticClient());
        services.AddSingleton<ElasticSearchService>();
        services.AddHostedService<IndexingWorker>();
    })
    .Build();

await host.RunAsync();
