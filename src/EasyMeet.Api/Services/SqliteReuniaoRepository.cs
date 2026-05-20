using System.Data;
using System.Globalization;
using System.Text.Json;
using EasyMeet.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace EasyMeet.Api.Services;

public sealed class SqliteReuniaoRepository : IReuniaoRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _connectionString;

    public SqliteReuniaoRepository(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredConnectionString = configuration.GetConnectionString("EasyMeet")
            ?? "Data Source=App_Data/easymeet.db";

        var builder = new SqliteConnectionStringBuilder(configuredConnectionString);
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(environment.ContentRootPath, builder.DataSource);
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = builder.ToString();
    }

    public async Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Reuniao (
                ID INTEGER NOT NULL CONSTRAINT PK_Reuniao PRIMARY KEY AUTOINCREMENT,
                Transcricao TEXT NOT NULL,
                Resumo TEXT NOT NULL,
                TopicosPrincipais TEXT NOT NULL DEFAULT '[]',
                Acoes TEXT NOT NULL DEFAULT '[]',
                Responsaveis TEXT NOT NULL DEFAULT '[]',
                Decisoes TEXT NOT NULL DEFAULT '[]',
                Pendencias TEXT NOT NULL DEFAULT '[]',
                DataReuniao TEXT NOT NULL DEFAULT '',
                TipoReuniao TEXT NOT NULL DEFAULT '',
                Confianca DECIMAL NOT NULL,
                GeradoPorIA INTEGER NOT NULL DEFAULT 0,
                ModoExecucao TEXT NOT NULL DEFAULT ''
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        await EnsureColumnAsync(connection, "TopicosPrincipais", "TEXT NOT NULL DEFAULT '[]'", cancellationToken);
        await EnsureColumnAsync(connection, "Acoes", "TEXT NOT NULL DEFAULT '[]'", cancellationToken);
        await EnsureColumnAsync(connection, "Responsaveis", "TEXT NOT NULL DEFAULT '[]'", cancellationToken);
        await EnsureColumnAsync(connection, "Decisoes", "TEXT NOT NULL DEFAULT '[]'", cancellationToken);
        await EnsureColumnAsync(connection, "Pendencias", "TEXT NOT NULL DEFAULT '[]'", cancellationToken);
        await EnsureColumnAsync(connection, "DataReuniao", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "TipoReuniao", "TEXT NOT NULL DEFAULT ''", cancellationToken);
        await EnsureColumnAsync(connection, "GeradoPorIA", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        await EnsureColumnAsync(connection, "ModoExecucao", "TEXT NOT NULL DEFAULT ''", cancellationToken);
    }

    public async Task<int> AdicionarAsync(Reuniao reuniao, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Reuniao (
                Transcricao,
                Resumo,
                TopicosPrincipais,
                Acoes,
                Responsaveis,
                Decisoes,
                Pendencias,
                DataReuniao,
                TipoReuniao,
                Confianca,
                GeradoPorIA,
                ModoExecucao)
            VALUES (
                $transcricao,
                $resumo,
                $topicosPrincipais,
                $acoes,
                $responsaveis,
                $decisoes,
                $pendencias,
                $dataReuniao,
                $tipoReuniao,
                $confianca,
                $geradoPorIA,
                $modoExecucao);

            SELECT last_insert_rowid();
            """;
        command.Parameters.Add(CreateParameter(command, "$transcricao", reuniao.Transcricao));
        command.Parameters.Add(CreateParameter(command, "$resumo", reuniao.Resumo));
        command.Parameters.Add(CreateParameter(command, "$topicosPrincipais", Serialize(reuniao.TopicosPrincipais)));
        command.Parameters.Add(CreateParameter(command, "$acoes", Serialize(reuniao.Acoes)));
        command.Parameters.Add(CreateParameter(command, "$responsaveis", Serialize(reuniao.Responsaveis)));
        command.Parameters.Add(CreateParameter(command, "$decisoes", Serialize(reuniao.Decisoes)));
        command.Parameters.Add(CreateParameter(command, "$pendencias", Serialize(reuniao.Pendencias)));
        command.Parameters.Add(CreateParameter(command, "$dataReuniao", reuniao.DataReuniao));
        command.Parameters.Add(CreateParameter(command, "$tipoReuniao", reuniao.TipoReuniao));
        command.Parameters.Add(CreateParameter(command, "$confianca", reuniao.Confianca, DbType.Decimal));
        command.Parameters.Add(CreateParameter(command, "$geradoPorIA", reuniao.GeradoPorIA ? 1 : 0, DbType.Int32));
        command.Parameters.Add(CreateParameter(command, "$modoExecucao", reuniao.ModoExecucao));

        var id = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(id, CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<Reuniao>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                ID,
                Transcricao,
                Resumo,
                TopicosPrincipais,
                Acoes,
                Responsaveis,
                Decisoes,
                Pendencias,
                DataReuniao,
                TipoReuniao,
                Confianca,
                GeradoPorIA,
                ModoExecucao
            FROM Reuniao
            ORDER BY ID DESC;
            """;

        var reunioes = new List<Reuniao>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            reunioes.Add(new Reuniao
            {
                Id = reader.GetInt32(0),
                Transcricao = reader.GetString(1),
                Resumo = reader.GetString(2),
                TopicosPrincipais = DeserializeList<string>(reader.GetString(3)),
                Acoes = DeserializeList<AcaoReuniaoItem>(reader.GetString(4)),
                Responsaveis = DeserializeList<string>(reader.GetString(5)),
                Decisoes = DeserializeList<string>(reader.GetString(6)),
                Pendencias = DeserializeList<string>(reader.GetString(7)),
                DataReuniao = reader.GetString(8),
                TipoReuniao = reader.GetString(9),
                Confianca = ReadDecimal(reader.GetValue(10)),
                GeradoPorIA = ReadBoolean(reader.GetValue(11)),
                ModoExecucao = reader.GetString(12)
            });
        }

        return reunioes;
    }

    private static async Task EnsureColumnAsync(
        SqliteConnection connection,
        string columnName,
        string definition,
        CancellationToken cancellationToken)
    {
        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT COUNT(1) FROM pragma_table_info('Reuniao') WHERE name = $columnName;";
        existsCommand.Parameters.Add(CreateParameter(existsCommand, "$columnName", columnName));

        var exists = Convert.ToInt32(await existsCommand.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture) > 0;
        if (exists)
        {
            return;
        }

        await using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE Reuniao ADD COLUMN {columnName} {definition};";
        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string Serialize<T>(IReadOnlyList<T> value)
    {
        return JsonSerializer.Serialize(value ?? [], JsonOptions);
    }

    private static IReadOnlyList<T> DeserializeList<T>(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<T>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static SqliteParameter CreateParameter(
        SqliteCommand command,
        string name,
        object value,
        DbType? dbType = null)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        if (dbType.HasValue)
        {
            parameter.DbType = dbType.Value;
        }

        return parameter;
    }

    private static decimal ReadDecimal(object value)
    {
        return value switch
        {
            decimal decimalValue => decimalValue,
            double doubleValue => Convert.ToDecimal(doubleValue),
            float floatValue => Convert.ToDecimal(floatValue),
            long longValue => longValue,
            int intValue => intValue,
            string text when decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0m
        };
    }

    private static bool ReadBoolean(object value)
    {
        return value switch
        {
            bool booleanValue => booleanValue,
            long longValue => longValue != 0,
            int intValue => intValue != 0,
            string text when bool.TryParse(text, out var parsed) => parsed,
            string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed != 0,
            _ => false
        };
    }
}
