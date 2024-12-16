using ocr_worker;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;

var worker = new OcrWorker();
worker.Start();

Console.WriteLine("OCR Worker is running. Press Ctrl+C to exit.");

while (true)
{
        Thread.Sleep(1000);
}
