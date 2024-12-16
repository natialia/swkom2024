using ocr_worker;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
internal class Program
{
    static void Main(string[] args)
    {
        var worker = new OcrWorker();
        worker.Start();

        Console.WriteLine("OCR Worker is running. Press Ctrl+C to exit.");

        while (true)
        {
                Thread.Sleep(1000);
        }
    }
}