using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Fic.XTB.FlowExecutionHistory.Models;
using McTools.Xrm.Connection;
using Newtonsoft.Json;

namespace Fic.XTB.FlowExecutionHistory.Extensions
{
    static class ConnectionDetailExtensions
    {
        private const string PASS_PHRASE = "MsCrmTools";
        private const string SALT = "Tanguy 92*";
        private const string HASH_ALGORITHM = "SHA1";
        private const string IV = "ahC3@bCa2Didfc3d";
        private const int KEY_SIZE = 256;
        private const int ITERATIONS = 2;
        public static AccessTokenResponse GetPowerAutomateAccessToken(this ConnectionDetail connection)
        {
            if (connection?.S2SClientSecret == null)
            {
                return null;
            }

            var secret = Decrypt(connection.S2SClientSecret);

            var url = $"https://login.microsoftonline.com/common/oauth2/v2.0/token";
            var data = new Dictionary<string, string>
            {
                ["client_id"] = connection.AzureAdAppId.ToString(),
                ["client_secret"] = secret,
                ["scope"] = $"https://service.flow.microsoft.com/.default",
                ["grant_type"] = "client_credentials",
            };

            var req = WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            using (var reqStream = req.GetRequestStream())
            using (var writer = new StreamWriter(reqStream))
            {
                writer.Write(string.Join("&", data.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}")));
            }

            using (var resp = req.GetResponse())
            using (var respStream = resp.GetResponseStream())
            using (var reader = new StreamReader(respStream))
            {
                var json = reader.ReadToEnd();
                var token = JsonConvert.DeserializeObject<AccessTokenResponse>(json);

                if (!string.IsNullOrEmpty(token?.refresh_token))
                    connection.RefreshToken = token.refresh_token;

                return token;
            }
        }

        private static string Decrypt(string cipherText)
        {
            using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
            {
                using (PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(
                           PASS_PHRASE,
                           Encoding.ASCII.GetBytes(SALT),
                           HASH_ALGORITHM,
                           ITERATIONS))
                {
                    var decryptor =
                        rijndaelManaged.CreateDecryptor(
                            passwordDeriveBytes.GetBytes(KEY_SIZE / 8),
                            Encoding.ASCII.GetBytes(IV));
                    var buffer = Convert.FromBase64String(cipherText);
                    using (var memoryStream = new MemoryStream(buffer))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        var numArray = new byte[buffer.Length];
                        var count = cryptoStream.Read(numArray, 0, numArray.Length);
                        return Encoding.UTF8.GetString(numArray, 0, count);
                    }
                }
            }
        }
    }
}
