using System.Data.SqlClient;

namespace API.DAL
{
    internal class Connection
    {
        public static SqlConnection GetConnection() => new (connectionString);

        private static readonly string connectionString = "workstation id=MyBussDB.mssql.somee.com;packet size=4096;user id=MyBuss_SQLLogin_1;pwd=8wopep96lt;data source=MyBussDB.mssql.somee.com;persist security info=False;initial catalog=MyBussDB";

    }
}
