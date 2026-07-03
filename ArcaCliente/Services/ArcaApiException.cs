using System;
using System.Collections.Generic;

namespace ArcaCliente.Services
{
    public class ArcaApiException : Exception
    {
        public int? StatusCode { get; }
        public List<string> Screenshots { get; }
        public bool IsTimeout { get; }

        public ArcaApiException(
            string message,
            int? statusCode = null,
            List<string> screenshots = null,
            bool isTimeout = false)
            : base(message)
        {
            StatusCode = statusCode;
            Screenshots = screenshots ?? new List<string>();
            IsTimeout = isTimeout;
        }

        public string UserFriendlyMessage => StatusCode switch
        {
            400 => $"Error en la solicitud: {Message}",
            401 => $"Token inválido o expirado. Volvé a iniciar sesión.",
            404 => $"No se encontró el recurso en el portal: {Message}",
            429 => "Límite de requests alcanzado. Obtené un nuevo token con Login.",
            _ when IsTimeout => "La operación tardó demasiado (timeout 120s). El portal ARCA puede estar lento, intentá de nuevo.",
            _ => $"Error inesperado: {Message}"
        };
    }
}
