using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Nohros.Logging;

namespace Nohros.Data.SqlServer
{
  internal class UpdateStateQuery
  {
    const string kClassName = "Nohros.Data.SqlServer.UpdateStateQuery";

    readonly MustLogger logger_ = MustLogger.ForCurrentProcess;
    readonly SqlConnectionProvider sql_connection_provider_;

    public UpdateStateQuery(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
      SupressTransactions = true;
    }

    public bool Execute(string name, string table_name, object state) {
      using (var scope =
        new TransactionScope(SupressTransactions
          ? TransactionScopeOption.Suppress
          : TransactionScopeOption.Required)) {
        using (
          SqlConnection conn = sql_connection_provider_.CreateConnection())
        using (var builder = new CommandBuilder(conn)) {
          IDbCommand cmd = builder
            .SetText(@"
update " + table_name + @"
set state = @state" + @"
where state_name = @name")
            .SetType(CommandType.Text)
            .AddParameter("@name", name)
            .AddParameterWithValue("@state", state)
            .Build();
          try {
            conn.Open();
            scope.Complete();
            return cmd.ExecuteNonQuery() > 0;
          } catch (SqlException e) {
            throw new ProviderException(e);
          }
        }
      }
    }

    public bool SupressTransactions { get; set; }
  }
}
