using EasyMeet.Api.Services;

namespace EasyMeet.Tests.Unit;

public sealed class MeetingAnalysisParserTests
{
    [Fact]
    public void ParseOrThrow_DeveMapearSchemaAtual_ComAcoesEstruturadas()
    {
        var parser = new MeetingAnalysisParser();
        var raw = """
        ```json
        {
          "resumo":"Resumo executivo",
          "topicosPrincipais":["Agenda","Agenda","Riscos"],
          "acoes":[
            {"descricao":"Atualizar cronograma","responsavel":"Ana","prazo":"2026-06-01"},
            {"descricao":"Atualizar cronograma","responsavel":"Ana","prazo":"2026-06-01"}
          ],
          "responsaveis":["Ana","Bruno"],
          "decisoesTomadas":["Aprovar plano"],
          "pendencias":["Validar orçamento"],
          "tipoReuniao":"técnica",
          "dataReuniao":"2026-05-20",
          "nivelConfianca":1.4
        }
        ```
        """;

        var result = parser.ParseOrThrow(raw);

        Assert.Equal("Resumo executivo", result.Resumo);
        Assert.Equal(["Agenda", "Riscos"], result.TopicosPrincipais);
        Assert.Single(result.Acoes);
        Assert.Equal("Atualizar cronograma", result.Acoes[0].Descricao);
        Assert.Equal("Ana", result.Acoes[0].Responsavel);
        Assert.Equal("2026-06-01", result.Acoes[0].Prazo);
        Assert.Equal(["Aprovar plano"], result.Decisoes);
        Assert.Equal("2026-05-20", result.DataReuniao);
        Assert.Equal("Tecnica", result.TipoReuniao);
        Assert.Equal(1d, result.NivelConfianca);
    }

    [Fact]
    public void ParseOrThrow_DeveAceitarSchemaLegado_DeDecisoesEAcoesTexto()
    {
        var parser = new MeetingAnalysisParser();
        var raw = """
        {
          "resumo":"Resumo legado",
          "topicosPrincipais":["Planejamento"],
          "acoes":["Criar ata"],
          "responsaveis":["Carlos"],
          "decisoes":["Manter escopo"],
          "pendencias":[],
          "tipoReuniao":"planning",
          "nivelConfianca":"0.75"
        }
        """;

        var result = parser.ParseOrThrow(raw);

        Assert.Equal("Resumo legado", result.Resumo);
        Assert.Single(result.Acoes);
        Assert.Equal("Criar ata", result.Acoes[0].Descricao);
        Assert.Equal("Nao identificado", result.Acoes[0].Responsavel);
        Assert.Equal(["Manter escopo"], result.Decisoes);
        Assert.Equal(string.Empty, result.DataReuniao);
        Assert.Equal("Planejamento", result.TipoReuniao);
        Assert.Equal(0.75d, result.NivelConfianca);
    }

    [Fact]
    public void ParseOrThrow_DeveFalhar_QuandoJsonNaoTemSchemaEsperado()
    {
        var parser = new MeetingAnalysisParser();

        var ex = Assert.Throws<InvalidOperationException>(() => parser.ParseOrThrow("""{"resumo":"incompleto"}"""));

        Assert.Contains("schema esperado", ex.Message);
    }

    [Fact]
    public void ParseOrThrow_DeveFalhar_QuandoNaoHaJson()
    {
        var parser = new MeetingAnalysisParser();

        var ex = Assert.Throws<InvalidOperationException>(() => parser.ParseOrThrow("resposta sem json"));

        Assert.Contains("extrair JSON", ex.Message);
    }
}
