using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using schedule_automation_app_server.WebAPI.Middleware;

namespace schedule_automation_app_server.Tests;

public class ExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext MakeContext()
    {
        DefaultHttpContext context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static IWebHostEnvironment MakeEnv(bool isDevelopment)
    {
        return isDevelopment ? new FakeEnvironment("Development") : new FakeEnvironment("Production");
    }

    private static async Task<(int statusCode, string body)> InvokeMiddleware(
        Exception? exToThrow,
        bool isDevelopment = false)
    {
        DefaultHttpContext context = MakeContext();
        IWebHostEnvironment env = MakeEnv(isDevelopment);

        RequestDelegate next = exToThrow == null ? _ => Task.CompletedTask : _ => throw exToThrow;

        ExceptionHandlingMiddleware middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance, env);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        string body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task InvokeAsync_NoException_Returns200()
    {
        (int statusCode, _) = await InvokeMiddleware(null);
        Assert.Equal(200, statusCode);
    }
    

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500()
    {
        (int statusCode, _) = await InvokeMiddleware(new InvalidOperationException("Ошибка"));
        Assert.Equal(500, statusCode);
    }
    

    [Fact]
    public async Task InvokeAsync_Response_HasJsonContentType()
    {
        DefaultHttpContext context = MakeContext();
        IWebHostEnvironment env = MakeEnv(false);
        RequestDelegate next = _ => throw new ArgumentException("Ошибка");

        ExceptionHandlingMiddleware middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance, env);

        await middleware.InvokeAsync(context);
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_Response_IsValidJson()
    {
        (_, string body) = await InvokeMiddleware(new ArgumentException("Тест"));
        JsonDocument? doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("error", out _));
    }

    private class FakeEnvironment : IWebHostEnvironment
    {
        public FakeEnvironment(string environmentName) { EnvironmentName = environmentName; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "TestApp";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}