﻿namespace LandingPage.Utils
{
    using System.Net.Mime;
    using System.Text.Json;
    using Microsoft.AspNetCore.StaticFiles;
    using Jose;

    internal static class MyExtensions
    {
        internal static string SerializeEncryptSign<T>(this T t, string secretKey) =>
            JWT.Encode(
               payload: JsonSerializer.Serialize(t),
               key: secretKey,
               alg: JweAlgorithm.PBES2_HS256_A128KW,
               enc: JweEncryption.A256CBC_HS512,
               compression: JweCompression.DEF);

        internal static T DecryptValidateDeserialize<T>(string token, string secretKey) =>
            JsonSerializer.Deserialize<T>(JWT.Decode(token, secretKey));

        internal static string DetermineContentTypeFromFilename(this string filename)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out string contentType))
            {
                contentType = MediaTypeNames.Application.Octet;
            }
            return contentType;
        }
    }
}