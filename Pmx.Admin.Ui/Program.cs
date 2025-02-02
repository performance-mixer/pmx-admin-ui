using Grpc.Core;
using Grpc.Net.Client;
using Pmx.Admin.Ui.Components;
using Pmx.Admin.Ui.Services;
using Pmx.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddTransient<ChannelBase>(_ =>
        GrpcChannel.ForAddress("http://localhost:50051"))
    .AddTransient<PmxGrpc.PmxGrpcClient>()
    .AddTransient<IPmxGrpcService, PmxGrpcService>()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();