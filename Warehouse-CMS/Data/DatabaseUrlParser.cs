namespace Warehouse_CMS.Data
{
    public static class DatabaseUrlParser
    {
        /// <summary>
        /// Parse a Railway/Heroku-style postgres:// URL into an Npgsql connection string.
        /// Uri.UserInfo is percent-encoded, so credentials must be unescaped; the password
        /// is optional and may itself contain ':'.
        /// </summary>
        public static string BuildNpgsqlConnectionString(string databaseUrl)
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password =
                userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

            var csb = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = username,
                Password = password,
                SslMode = Npgsql.SslMode.Require,
            };

            return csb.ConnectionString;
        }
    }
}
