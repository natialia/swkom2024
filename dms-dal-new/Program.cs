using Microsoft.EntityFrameworkCore;
using dms_dal_new.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
namespace dms_dal_new
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
