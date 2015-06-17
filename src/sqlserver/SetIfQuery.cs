using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Nohros.Data;
using Nohros.Data.SqlServer.Extensions;
using Nohros.Logging;

namespace Nohros.Data.SqlServer
{
  public class SetIfQuery
  {
    const string kClassName = "Nohros.Data.SqlCe.SetIfGreaterThanQuery";

    readonly MustLogger logger_ = MustLogger.ForCurrentProcess;
    readonly SqlConnectionProvider sql_connection_provider_;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetIfGreaterThanQuery"/>
    /// using the given <param ref="sql_connection_provider" />
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> object that can be used to
    /// create connections to the data provider.
    /// </param>
    public SetIfQuery(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
      SupressTransactions = true;
    }

    public bool Execute<T>(ComparisonOperator op, string name, string table_name,
      T state, T comparand) {
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
where state_name " + op.Symbol() + @" @name
  and state = @comparand")
            .SetType(CommandType.Text)
            .AddParameter("@name", name)
            .AddParameterWithValue("@state", state)
            .AddParameterWithValue("@comparand", comparand)
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
