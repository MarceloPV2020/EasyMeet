using System.Text.Json;
using EasyMeet.Api.Models;

namespace EasyMeet.Api.Services;

public sealed class MeetingAnalysisParser
{
    public ResumoReuniaoIaOutput ParseOrThrow(string rawText)
    {
        var cleanedJson = TryExtractJsonObject(rawText);
        if (string.IsNullOrWhiteSpace(cleanedJson))
        {
            throw new InvalidOperationException("Nao foi possivel extrair JSON valido da resposta da IA.");
        }

        using var doc = JsonDocument.Parse(cleanedJson);
        var root = doc.RootElement;
        if (!HasExpectedShape(root))
        {
            throw new InvalidOperationException("JSON da IA invalido ou fora do schema esperado.");
        }

        var resumo = root.GetProperty("resumo").GetString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(resumo))
        {
            throw new InvalidOperationException("Conteudo da IA invalido.");
        }

        var tipoReuniao = root.GetProperty("tipoReuniao").GetString();
        var nivelConfianca = ExtractConfianca(root.GetProperty("nivelConfianca"));

        return new ResumoReuniaoIaOutput
        {
            Resumo = resumo,
            TopicosPrincipais = ExtractStringArray(root.GetProperty("topicosPrincipais")),
            Acoes = ExtractAcoes(root.GetProperty("acoes")),
            Responsaveis = ExtractStringArray(root.GetProperty("responsaveis")),
            Decisoes = ExtractDecisoes(root),
            Pendencias = ExtractStringArray(root.GetProperty("pendencias")),
            TipoReuniao = NormalizeTipoReuniao(tipoReuniao),
            NivelConfianca = nivelConfianca
        };
    }

    private static IReadOnlyList<AcaoReuniaoItem> ExtractAcoes(JsonElement acoesNode)
    {
        if (acoesNode.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<AcaoReuniaoItem>();
        foreach (var item in acoesNode.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                var descricao = item.TryGetProperty("descricao", out var d) ? d.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(descricao))
                {
                    continue;
                }

                var responsavel = item.TryGetProperty("responsavel", out var r) ? r.GetString() ?? string.Empty : string.Empty;
                var prazo = item.TryGetProperty("prazo", out var p) ? p.GetString() ?? string.Empty : string.Empty;

                result.Add(new AcaoReuniaoItem
                {
                    Descricao = descricao.Trim(),
                    Responsavel = string.IsNullOrWhiteSpace(responsavel) ? "Nao identificado" : responsavel.Trim(),
                    Prazo = prazo.Trim()
                });
            }
            else if (item.ValueKind == JsonValueKind.String)
            {
                var text = item.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    result.Add(new AcaoReuniaoItem
                    {
                        Descricao = text.Trim(),
                        Responsavel = "Nao identificado",
                        Prazo = string.Empty
                    });
                }
            }
        }

        return result
            .GroupBy(x => $"{x.Descricao}|{x.Responsavel}|{x.Prazo}", StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();
    }

    private static IReadOnlyList<string> ExtractDecisoes(JsonElement root)
    {
        if (root.TryGetProperty("decisoesTomadas", out var decisoesTomadas) && decisoesTomadas.ValueKind == JsonValueKind.Array)
        {
            return ExtractStringArray(decisoesTomadas);
        }

        if (root.TryGetProperty("decisoes", out var decisoes) && decisoes.ValueKind == JsonValueKind.Array)
        {
            return ExtractStringArray(decisoes);
        }

        return [];
    }

    private static IReadOnlyList<string> ExtractStringArray(JsonElement arrayNode)
    {
        if (arrayNode.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return arrayNode.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString() ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static double ExtractConfianca(JsonElement node)
    {
        if (node.ValueKind == JsonValueKind.Number && node.TryGetDouble(out var number))
        {
            return Math.Clamp(number, 0d, 1d);
        }

        if (node.ValueKind == JsonValueKind.String && double.TryParse(node.GetString(), out var parsed))
        {
            return Math.Clamp(parsed, 0d, 1d);
        }

        return 0d;
    }

    private static string? TryExtractJsonObject(string rawText)
    {
        var cleaned = rawText.Trim();
        if (cleaned.StartsWith("```", StringComparison.Ordinal))
        {
            cleaned = cleaned.Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("```", string.Empty, StringComparison.Ordinal)
                .Trim();
        }

        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');
        if (start < 0 || end < 0 || end <= start)
        {
            return null;
        }

        return cleaned[start..(end + 1)];
    }

    private static bool HasExpectedShape(JsonElement root)
    {
        var hasDecisoes = root.TryGetProperty("decisoesTomadas", out var decisoesTomadas) && decisoesTomadas.ValueKind == JsonValueKind.Array
            || root.TryGetProperty("decisoes", out var decisoes) && decisoes.ValueKind == JsonValueKind.Array;

        return root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("resumo", out var resumo) && resumo.ValueKind == JsonValueKind.String
            && root.TryGetProperty("topicosPrincipais", out var topicos) && topicos.ValueKind == JsonValueKind.Array
            && root.TryGetProperty("acoes", out var acoes) && acoes.ValueKind == JsonValueKind.Array
            && root.TryGetProperty("responsaveis", out var responsaveis) && responsaveis.ValueKind == JsonValueKind.Array
            && hasDecisoes
            && root.TryGetProperty("pendencias", out var pendencias) && pendencias.ValueKind == JsonValueKind.Array
            && root.TryGetProperty("tipoReuniao", out var tipo) && tipo.ValueKind == JsonValueKind.String
            && root.TryGetProperty("nivelConfianca", out var confianca) && (confianca.ValueKind == JsonValueKind.Number || confianca.ValueKind == JsonValueKind.String);
    }

    private static string NormalizeTipoReuniao(string? tipoReuniao)
    {
        if (string.IsNullOrWhiteSpace(tipoReuniao))
        {
            return "Desconhecida";
        }

        var normalized = tipoReuniao.Trim().ToLowerInvariant();
        return normalized switch
        {
            "daily" or "diaria" or "diária" => "Diaria",
            "planning" or "planejamento" => "Planejamento",
            "status" => "Status",
            "retrospective" or "retrospectiva" => "Retrospectiva",
            "oneonone" or "one-on-one" or "one on one" or "1:1" or "um a um" => "Um a Um",
            "incident" or "incidente" => "Incidente",
            "executive" or "executiva" => "Executiva",
            "commercial" or "comercial" => "Comercial",
            "technical" or "tecnica" or "técnica" => "Tecnica",
            "alignment" or "alinhamento" => "Alinhamento",
            "unknown" or "desconhecida" => "Desconhecida",
            _ => tipoReuniao.Trim()
        };
    }
}

