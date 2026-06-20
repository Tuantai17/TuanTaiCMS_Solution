using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CMS.Backend.Helpers
{
    public static class CustomerSessionTokenHelper
    {
        private sealed class TokenPayload
        {
            public int CustomerId { get; set; }
            public string Email { get; set; } = string.Empty;
            public long ExpiresAtUnix { get; set; }
        }

        public static string GenerateToken(int customerId, string email, string secret, DateTimeOffset expiresAt)
        {
            var payload = new TokenPayload
            {
                CustomerId = customerId,
                Email = email,
                ExpiresAtUnix = expiresAt.ToUnixTimeSeconds()
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            var signature = ComputeSignature(payloadBytes, secret);

            return $"{Base64UrlEncode(payloadBytes)}.{Base64UrlEncode(signature)}";
        }

        public static bool TryValidateToken(string token, string secret, out int customerId)
        {
            customerId = 0;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            try
            {
                var payloadBytes = Base64UrlDecode(parts[0]);
                var signatureBytes = Base64UrlDecode(parts[1]);
                var expectedSignature = ComputeSignature(payloadBytes, secret);

                if (!CryptographicOperations.FixedTimeEquals(signatureBytes, expectedSignature))
                {
                    return false;
                }

                var payload = JsonSerializer.Deserialize<TokenPayload>(payloadBytes);
                if (payload == null || payload.CustomerId <= 0)
                {
                    return false;
                }

                var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (payload.ExpiresAtUnix <= nowUnix)
                {
                    return false;
                }

                customerId = payload.CustomerId;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ComputeSignature(byte[] payloadBytes, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return hmac.ComputeHash(payloadBytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var normalized = input.Replace('-', '+').Replace('_', '/');
            var padding = normalized.Length % 4;
            if (padding > 0)
            {
                normalized = normalized.PadRight(normalized.Length + (4 - padding), '=');
            }

            return Convert.FromBase64String(normalized);
        }
    }
}
