using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
#pragma warning disable ASPDEPR008
#pragma warning disable ASPDEPR004

public class Usage
{
    static async Task Foo(HttpClient httpClient)
    {
        #region ClientFormSender

        var clientFormSender = new ClientFormSender(httpClient);
        await clientFormSender.Send(
            route: "/SendMessage",
            message: "{\"Property\": \"Value\"}",
            typeName: "TheMessageType",
            typeNamespace: "TheMessageNamespace",
            destination: "TheDestination",
            attachments: new()
            {
                {"fileName", "fileContents"u8.ToArray()}
            });

        #endregion
    }

    static async Task SubmitMultipartForm()
    {
        #region asptesthost

        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        using var testServer = new TestServer(hostBuilder);
        using var httpClient = testServer.CreateClient();
        var clientFormSender = new ClientFormSender(httpClient);
        await clientFormSender.Send(
            route: "/SendMessage",
            message: "{\"Property\": \"Value\"}",
            typeName: "TheMessageType",
            typeNamespace: "TheMessageNamespace",
            destination: "TheDestination",
            attachments: new()
            {
                {"fileName", "fileContents"u8.ToArray()}
            });

        #endregion
    }
}