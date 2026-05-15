using Microsoft.EntityFrameworkCore;
using DoAnCS.Data;
using DoAnCS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

var builder = Host.CreateApplicationBuilder(args);
// Re-check connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=.;Database=DoAnWebCS;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"));

using IHost host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var resumes = await context.Resumes
        .Where(r => r.IsPublic && !string.IsNullOrEmpty(r.JsonContent))
        .OrderByDescending(r => r.ResumeID)
        .Take(3)
        .ToListAsync();
    
    foreach(var r in resumes) {
        Console.WriteLine($"--- ID: {r.ResumeID} | Name: {r.FullName} | Title: {r.JobTitle} ---");
        Console.WriteLine($"Summary: {r.Summary}");
        // Just a snippet of content
        if (r.JsonContent.Length > 200) 
            Console.WriteLine($"Content Snippet: {r.JsonContent.Substring(0, 200)}...");
        else
            Console.WriteLine($"Content: {r.JsonContent}");
        Console.WriteLine();
    }
}
