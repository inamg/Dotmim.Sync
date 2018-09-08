﻿using Dotmim.Sync.MySql;
using Dotmim.Sync.SqlServer;
using Dotmim.Sync.Tests.Core;
using Dotmim.Sync.Tests.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dotmim.Sync.Tests.MySql
{
    /// <summary>
    /// Fixture used to test the SqlSyncProvider
    /// </summary>
    public class MySqlFixture : ProviderFixture<CoreProvider>
    {
        public override string DatabaseName => "mysqladventureworks";

        public override ProviderType ProviderType => ProviderType.MySql;

        public override bool EnableSqlServerClientOnTcp => true;
        public override bool EnableSqlServerClientOnHttp => false;
        public override bool EnableOracleClientOnTcp => false;
        public override bool EnableOracleClientOnHttp => false;
        public override bool EnableMySqlClientOnTcp => true;
        public override bool EnableMySqlClientOnHttp => false;
        public override bool EnableSqliteClientOnTcp => false;
        public override bool EnableSqliteClientOnHttp => true;

        // for debugging
        //public override bool DeleteAllDatabasesOnDispose => false;

        public override CoreProvider NewServerProvider(string connectionString)
        {
            return new MySqlSyncProvider(connectionString);
        }
        
    }
}