using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InstaTransfer.ITLogic.Helpers
{
    public class ApiHelper
    {
        /// <summary>
        /// Devuelve un <see cref="JwtSecurityToken"/> desde el header del request
        /// </summary>
        /// <param name="request">El request que contiene el token en el header</param>
        /// <returns><see cref="JwtSecurityToken"/> del request</returns>
        public static JwtSecurityToken ReadTokenFromHeader(HttpRequestMessage request)
        {
            IEnumerable<string> authHeaderValues;
            request.Headers.TryGetValues("Authorization", out authHeaderValues);
            var bearerToken = authHeaderValues.ElementAt(0);
            var plainToken = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.ReadJwtToken(plainToken);

            return jwtSecurityToken;
        }
        public static byte[] GetFileCompressedBytes(string filePath)
        {
            try
            {
                // Leemos todos los bytes del archivo
                var bytes = File.ReadAllBytes(filePath);
                // Comprimimos los bytes
                byte[] compress = Compress(bytes);
                // Success
                return compress;

            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Compresses byte array to new byte array.
        /// </summary>
        public static byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory,
                    CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
                CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

    }
}
