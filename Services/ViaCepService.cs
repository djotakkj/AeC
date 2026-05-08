using AeC.Enderecos.Models;
using System.Text.Json;

namespace AeC.Enderecos.Services;

// Serviço responsável por consultar a API pública ViaCEP
// Documentação: https://viacep.com.br/
public class ViaCepService
{
    private readonly HttpClient _httpClient;

    // HttpClient é injetado pelo ASP.NET via AddHttpClient<ViaCepService>() no Program.cs
    public ViaCepService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Busca endereço pelo CEP informado
    // Retorna null se o CEP for inválido ou não encontrado
    public async Task<ViaCepResponse?> BuscarEnderecoPorCepAsync(string cep)
    {
        // Remove traços e espaços, aceita tanto "12345-678" quanto "12345678"
        cep = cep.Replace("-", "").Trim();

        // CEP deve ter exatamente 8 dígitos
        if (cep.Length != 8 || !cep.All(char.IsDigit))
            return null;

        var resposta = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/");

        if (!resposta.IsSuccessStatusCode)
            return null;

        var json = await resposta.Content.ReadAsStringAsync();

        var dados = JsonSerializer.Deserialize<ViaCepResponse>(json);

        // A API do ViaCEP retorna {"erro": true} quando o CEP não existe
        // Verificamos tanto bool quanto string pois a API pode variar
        if (dados == null || dados.Erro != null)
            return null;

        return dados;
    }
}
