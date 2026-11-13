using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StaffManagement.Controllers;
using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement;
using Xunit;

namespace StaffManagement.Tests;

public sealed class LeaveIndexViewTests
{
    [Fact]
    public async Task Index_shows_applied_count_status_and_action_headers_with_no_requests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());
        services.AddControllersWithViews()
            .AddApplicationPart(typeof(LeaveController).Assembly);

        using var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        var routeData = new RouteData();
        routeData.Values["controller"] = "Leave";
        routeData.Values["action"] = "Index";
        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        var viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
        var viewResult = viewEngine.FindView(actionContext, "Leave/Index", isMainPage: true);

        Assert.True(viewResult.Success, $"Leave/Index view not found. Searched: {string.Join(", ", viewResult.SearchedLocations ?? [])}");

        var viewData = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = new PagedResultDto<LeaveRequestDto>
            {
                Items = Array.Empty<LeaveRequestDto>(),
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 0
            }
        };
        var tempData = new TempDataDictionary(httpContext, new EmptyTempDataProvider());
        using var writer = new StringWriter(new StringBuilder());
        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);

        var renderedHtml = writer.ToString();
        Assert.Contains("Applied count", renderedHtml);
        Assert.Contains("Status", renderedHtml);
        Assert.Contains("Action", renderedHtml);
    }

    private sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = typeof(LeaveController).Assembly.GetName().Name!;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
