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
                TopicosPrincipais = ["Topico A", "Topico B"],
                Acoes =
                [
                    new AcaoReuniaoItem
                    {
                        Descricao = "Executar plano",
                        Responsavel = "Ana",
                        Prazo = "2026-06-01"
                    }
                ],
                Responsaveis = ["Ana", "Bruno"],
                Decisoes = ["Aprovar plano"],
                Pendencias = ["Validar custo"],
                DataReuniao = "2026-05-20",
                TipoReuniao = "Planejamento",
                Confianca = 0.91m,
                GeradoPorIA = true,
                ModoExecucao = "Gemini",
                ModeloIA = "gemini-2.5-flash"
            });

            var reunioes = await repository.ListarAsync();

            var reuniao = Assert.Single(reunioes);
            Assert.Equal(id, reuniao.Id);
            Assert.Equal("Transcricao completa", reuniao.Transcricao);
            Assert.Equal("Resumo completo", reuniao.Resumo);
            Assert.Equal(["Topico A", "Topico B"], reuniao.TopicosPrincipais);
            var acao = Assert.Single(reuniao.Acoes);
            Assert.Equal("Executar plano", acao.Descricao);
            Assert.Equal("Ana", acao.Responsavel);
            Assert.Equal("2026-06-01", acao.Prazo);
            Assert.Equal(["Ana", "Bruno"], reuniao.Responsaveis);
            Assert.Equal(["Aprovar plano"], reuniao.Decisoes);
            Assert.Equal(["Validar custo"], reuniao.Pendencias);
            Assert.Equal("2026-05-20", reuniao.DataReuniao);
            Assert.Equal("Planejamento", reuniao.TipoReuniao);
            Assert.Equal(0.91m, reuniao.Confianca);
            Assert.True(reuniao.GeradoPorIA);
            Assert.Equal("Gemini", reuniao.ModoExecucao);
            Assert.Equal("gemini-2.5-flash", reuniao.ModeloIA);
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

    [Fact]
    public async Task Repositorio_DeveMigrarBancoExistenteComSomenteCamposBasicos()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "EasyMeetTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var databasePath = Path.Combine(tempDirectory, "easymeet-test.db");

        try
        {
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE Reuniao (
                        ID INTEGER NOT NULL CONSTRAINT PK_Reuniao PRIMARY KEY AUTOINCREMENT,
                        Transcricao TEXT NOT NULL,
                        Resumo TEXT NOT NULL,
                        Confianca DECIMAL NOT NULL
                    );
                    """;
                await command.ExecuteNonQueryAsync();
            }

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
            await repository.AdicionarAsync(new Reuniao
            {
                Transcricao = "Transcricao migrada",
                Resumo = "Resumo migrado",
                TopicosPrincipais = ["Topico migrado"],
                DataReuniao = "2026-05-21",
                TipoReuniao = "Status",
                Confianca = 0.88m,
                GeradoPorIA = true,
                ModoExecucao = "Groq",
                ModeloIA = "llama-3.1-8b-instant"
            });

            var reuniao = Assert.Single(await repository.ListarAsync());
            Assert.Equal(["Topico migrado"], reuniao.TopicosPrincipais);
            Assert.Equal("2026-05-21", reuniao.DataReuniao);
            Assert.Equal("Status", reuniao.TipoReuniao);
            Assert.Equal("Groq", reuniao.ModoExecucao);
            Assert.Equal("llama-3.1-8b-instant", reuniao.ModeloIA);
            Assert.True(reuniao.GeradoPorIA);
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

    [Fact]
    public async Task Repositorio_DeveRemoverReuniaoPorId()
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
                Transcricao = "Transcricao para remover",
                Resumo = "Resumo para remover",
                Confianca = 0.8m
            });

            var removed = await repository.RemoverAsync(id);
            var removedAgain = await repository.RemoverAsync(id);

            Assert.True(removed);
            Assert.False(removedAgain);
            Assert.Empty(await repository.ListarAsync());
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
