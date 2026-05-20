using EasyMeet.Api.Models;
using EasyMeet.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace EasyMeet.Tests.Unit;

public sealed class SqliteReuniaoRepositoryTests
{
    [Fact]
    public async Task Repositorio_DeveCriarTabelaInserirEListarReunioes()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "EasyMeetTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:EasyMeet"] = "Data Source=easymeet-test.db"
                })
                .Build();
            var repository = new SqliteReuniaoRepository(configuration, new FakeWebHostEnvironment
            {
                ContentRootPath = tempDirectory
            });

            await repository.InicializarAsync();
            var id = await repository.AdicionarAsync(new Reuniao
            {
                Transcricao = "Transcricao completa",
                Resumo = "Resumo completo",
                Confianca = 0.91m
            });

            var reunioes = await repository.ListarAsync();

            var reuniao = Assert.Single(reunioes);
            Assert.Equal(id, reuniao.Id);
            Assert.Equal("Transcricao completa", reuniao.Transcricao);
            Assert.Equal("Resumo completo", reuniao.Resumo);
            Assert.Equal(0.91m, reuniao.Confianca);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "EasyMeet.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
