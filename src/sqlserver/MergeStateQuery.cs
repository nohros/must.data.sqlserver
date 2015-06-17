using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Nohros.Data.SqlServer.Extensions;
using Nohros.Logging;

namespace Nohros.Data.SqlServer
{
  public class MergeStateQuery
  {
    const string kClassName = "Nohros.Data.SqlCe.SetIfGreaterThanQuery";

    readonly MustLogger logger_ = MustLogger.ForCurrentProcess;
    readonly SqlConnectionProvider sql_connection_provider_;

    /// <summary>
    /// Initializes a new instance of the <see cref="MergeStateQuery"/>
    /// using the given <param ref="sql_connection_provider" />
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> object that can be used to
    /// create connections to the data provider.
    /// </param>
    public MergeStateQuery(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
      SupressTransactions = true;
    }

    public bool Execute<T>(string name, string table_name, T state) {
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
set state = state + @state" + @"
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
            throw e.AsProviderException();
          }
        }
      }
    }

    public bool SupressTransactions { get; set; }
  }
}
