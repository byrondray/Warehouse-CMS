using Npgsql;
using Warehouse_CMS.Data;
using Xunit;

namespace Warehouse_CMS.Tests
{
    public class DatabaseUrlParserTests
    {
        private static NpgsqlConnectionStringBuilder Parse(string url) =>
            new(DatabaseUrlParser.BuildNpgsqlConnectionString(url));

        [Fact]
        public void ParsesHostPortDatabaseAndCredentials()
        {
            var csb = Parse("postgres://user:pass@dbhost:6543/mydb");

            Assert.Equal("dbhost", csb.Host);
            Assert.Equal(6543, csb.Port);
            Assert.Equal("mydb", csb.Database);
            Assert.Equal("user", csb.Username);
            Assert.Equal("pass", csb.Password);
            Assert.Equal(SslMode.Require, csb.SslMode);
        }

        [Fact]
        public void DefaultsPortTo5432WhenAbsent()
        {
            var csb = Parse("postgres://user:pass@dbhost/mydb");

            Assert.Equal(5432, csb.Port);
        }

        [Fact]
        public void PasswordContainingColonIsPreserved()
        {
            // The password may itself contain ':'; only the first ':' separates user from password.
            var csb = Parse("postgres://user:pa:ss:word@dbhost/mydb");

            Assert.Equal("user", csb.Username);
            Assert.Equal("pa:ss:word", csb.Password);
        }

        [Fact]
        public void PercentEncodedCredentialsAreUnescaped()
        {
            // "p@ss word" encodes to "p%40ss%20word".
            var csb = Parse("postgres://us%65r:p%40ss%20word@dbhost/mydb");

            Assert.Equal("user", csb.Username);
            Assert.Equal("p@ss word", csb.Password);
        }

        [Fact]
        public void EmptyPasswordIsHandled()
        {
            var csb = Parse("postgres://user@dbhost/mydb");

            Assert.Equal("user", csb.Username);
            // No password in the URL means no Password key is emitted (reads back as null).
            Assert.True(string.IsNullOrEmpty(csb.Password));
        }

        [Fact]
        public void MalformedUrlThrows()
        {
            Assert.Throws<UriFormatException>(
                () => DatabaseUrlParser.BuildNpgsqlConnectionString("not-a-valid-url")
            );
        }
    }
}
