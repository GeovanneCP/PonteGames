using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PonteGames.Api.DTOs;
using Xunit;

namespace PonteGames.Tests;

public class DescarteEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DescarteEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> ObterTokenJwtAsync()
    {
        var loginPayload = new LoginRequestDto("geovanne@pontegames.com");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        return loginResult!.Token;
    }

    [Fact]
    public async Task GetHealth_DeveRetornarStatus200Ok()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRanking_DeveRetornarListaDeUsuariosComSucesso()
    {
        var response = await _client.GetAsync("/api/ranking");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostDescarte_SemAutenticacao_DeveRetornarUnauthorized()
    {
        var payload = new RegistroDescarteDto(1, "plastico", 2.0);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/descartes")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostDescarte_SemCabecalhoIdempotencyKey_DeveRetornarBadRequest()
    {
        var token = await ObterTokenJwtAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new RegistroDescarteDto(1, "eletronico", 2.5);
        var response = await _client.PostAsJsonAsync("/api/descartes", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostDescarte_ComMesmaIdempotencyKeyDuasVezes_DeveRetornarConflictNaSegunda()
    {
        var token = await ObterTokenJwtAsync();
        var chaveUnica = Guid.NewGuid().ToString();
        var payload = new RegistroDescarteDto(1, "metal", 1.5);

        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/descartes")
        {
            Content = JsonContent.Create(payload)
        };
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request1.Headers.Add("X-Idempotency-Key", chaveUnica);

        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/descartes")
        {
            Content = JsonContent.Create(payload)
        };
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request2.Headers.Add("X-Idempotency-Key", chaveUnica);

        // Primeiro envio: Criado (201)
        var response1 = await _client.SendAsync(request1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        // Segundo envio com a mesma chave: Bloqueado pelo filtro (409)
        var response2 = await _client.SendAsync(request2);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }
}