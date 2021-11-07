﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static void Main()
    {
        var builder = WebHost.CreateDefaultBuilder();
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        builder.UseStartup<SampleStartup>();
        var webHost = builder.Build();
        webHost.Run();
    }
}