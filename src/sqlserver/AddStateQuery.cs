using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Nohros.Data;
using Nohros.Data.SqlServer;
using Nohros.Data.SqlServer.Extensions;
using Nohros.Extensions;
using Nohros.Logging;
using Nohros.Resources;

namespace Nohros.Data.SqlServer
{
  public class AddStateQuery
  {
    const string kClassName = "Nohros.Data.SqlServer.AddStateQuery";

    readonly MustLogger logger_ = MustLogger.ForCurrentProcess;
    readonly SqlConnectionProvider sql_connection_provider_;

    public AddStateQuery(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
      SupressTransactions = true;
    }

    public void Execute(string state_name, string table_name, object state) {
      using (var scope =
        new TransactionScope(SupressTransactions
          ? TransactionScopeOption.Suppress
          : TransactionScopeOption.Required)) {
        using (SqlConnection conn = sql_connection_provider_.CreateConnection())
        using (var builder = new CommandBuilder(conn)) {
          IDbCommand cmd = builder
            .SetText(@"
insert into " + table_name + @"(state_name, state)
values(@name, @state)")
            .SetType(CommandType.Text)
            .AddParameter("@name", state_name)
            .AddParameterWithValue("@state", state)
            .Build();
          try {
            conn.Open();
            cmd.ExecuteNonQuery();
            scope.Complete();
          } catch (SqlException e) {
            throw e.AsProviderException();
          }
        }
      }
    }

    public bool SupressTransactions { get; set; }
  }
}
