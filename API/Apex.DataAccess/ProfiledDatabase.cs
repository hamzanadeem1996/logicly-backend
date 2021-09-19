using NPoco;
using StackExchange.Profiling;
using System;
using System.Data;
using System.Data.Common;

namespace Apex.DataAccess
{
    public class ProfiledDatabase : Database
    {
        public ProfiledDatabase(IDbConnection connection) : base((DbConnection)connection)
        {
        }

        public ProfiledDatabase(string connectionString, DatabaseType type, DbProviderFactory dbProviderFactory) :
            base(connectionString, type, dbProviderFactory)
        { }

        protected override void OnException(Exception ex)
        {
            base.OnException(ex);
            ex.Data["LastSQL"] = this.LastSQL;
        }

        protected override DbConnection OnConnectionOpened(DbConnection connection)
        {
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection as DbConnection, MiniProfiler.Current);
        }
    }
}