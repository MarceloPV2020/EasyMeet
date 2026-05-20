using System.Text.Json.Serialization;
using EasyMeet.Api.Models;
using EasyMeet.Api.Prompts;
using EasyMeet.Api.Providers;
using EasyMeet.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EasyMeet API",
        Version = "v1",
        Description = "API para analise de reunioes com provedores de IA selecionaveis."
    });
});

builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection(GeminiSettings.SectionName));
builder.Services.Configure<GroqSettings>(builder.Configuration.GetSection(GroqSettings.SectionName));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection(OpenAISettings.SectionName));
builder.Services.Configure<AnthropicSettings>(builder.Configuration.GetSection(AnthropicSettings.SectionName));
builder.Services.Configure<MistralSettings>(builder.Configuration.GetSection(MistralSettings.SectionName));
builder.Services.Configure<CohereSettings>(builder.Configuration.GetSection(CohereSettings.SectionName));
builder.Services.Configure<AzureOpenAISettings>(builder.Configuration.GetSection(AzureOpenAISettings.SectionName));
builder.Services.AddScoped<PromptResumoReuniaoBuilder>();
builder.Services.AddScoped<MeetingAnalysisParser>();
builder.Services.AddScoped<IApiKeyStore, WindowsCredentialApiKeyStore>();
builder.Services.AddScoped<IAProviderFactory, AIProviderFactory>();
builder.Services.AddScoped<ApiKeyManagerService>();
builder.Services.AddScoped<IAgenteResumoReuniaoService, AgenteResumoReuniaoService>();

builder.Services.AddHttpClient<GeminiClientService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<GeminiClientService>());

builder.Services.AddHttpClient<GroqClientService>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<GroqClientService>());

builder.Services.AddHttpClient<OpenAIClientService>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<OpenAIClientService>());

builder.Services.AddHttpClient<AnthropicClientService>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<AnthropicClientService>());

builder.Services.AddHttpClient<MistralClientService>(client =>
{
    client.BaseAddress = new Uri("https://api.mistral.ai/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<MistralClientService>());

builder.Services.AddHttpClient<CohereClientService>(client =>
{
    client.BaseAddress = new Uri("https://api.cohere.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<CohereClientService>());

builder.Services.AddHttpClient<AzureOpenAIClientService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IGenerativeAIClient>(sp => sp.GetRequiredService<AzureOpenAIClientService>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "EasyMeet.Api" }));
app.MapControllers();

app.Run();

public partial class Program;
