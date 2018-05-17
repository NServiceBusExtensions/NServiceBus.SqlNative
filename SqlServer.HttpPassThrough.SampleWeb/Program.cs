using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        var webHost = WebHostBuilder.Build();


        webHost.Run();
    }
}