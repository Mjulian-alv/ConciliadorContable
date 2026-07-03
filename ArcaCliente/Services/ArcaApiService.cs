using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ArcaCliente.Models;

namespace ArcaCliente.Services
{
    public sealed class ArcaApiService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private string _apiToken;
        private bool _disposed;

        public ArcaApiService(string baseUrl, string apiToken = null)
        {
            // Acepta certificados de desarrollo (localhost). Cambiar en producción.
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(120)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            _apiToken = apiToken;
        }

        public void SetToken(string token) => _apiToken = token;

        public bool HasToken => !string.IsNullOrWhiteSpace(_apiToken);

        // ?? Auth ????????????????????????????????????????????????????????????????

        public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
            => PostAsync<RegisterRequest, AuthResponse>("api/auth/register", request, requiresAuth: false, ct);

        public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
            => PostAsync<LoginRequest, AuthResponse>("api/auth/login", request, requiresAuth: false, ct);

        // ?? Negocio ?????????????????????????????????????????????????????????????

        public Task<ExportarResponse> ExportarComprobantesAsync(ExportarRequest request, CancellationToken ct = default)
            => PostAsync<ExportarRequest, ExportarResponse>("api/MisComprobantes/exportar", request, requiresAuth: true, ct);

        // ?? Núcleo HTTP ?????????????????????????????????????????????????????????

        private async Task<TResponse> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest body,
            bool requiresAuth,
            CancellationToken ct)
            where TResponse : ArcaBaseResponse
        {
            if (requiresAuth && string.IsNullOrWhiteSpace(_apiToken))
                throw new ArcaApiException("No hay token configurado. Registrate o hacé login primero.");

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(body, options: _jsonOptions)
            };
            
            if (requiresAuth)
                request.Headers.Add("X-Api-Token", _apiToken);

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.SendAsync(request, ct);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new ArcaApiException(
                    "La solicitud tardó demasiado (timeout de 120 segundos).",
                    isTimeout: true);
            }
            catch (HttpRequestException ex)
            {
                throw new ArcaApiException($"Error de conexión con el servidor: {ex.Message}");
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync(ct);

            TResponse result;
            try
            {
                result = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions)
                    ?? throw new ArcaApiException("El servidor devolvió una respuesta vacía.");
            }
            catch (JsonException)
            {
                throw new ArcaApiException(
                    $"No se pudo interpretar la respuesta del servidor. HTTP {(int)httpResponse.StatusCode}: {responseContent}");
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                var screenshots = (result as ExportarResponse)?.Screenshots;
                throw new ArcaApiException(
                    result.Message ?? "Error desconocido",
                    (int)httpResponse.StatusCode,
                    screenshots);
            }

            return result;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}
