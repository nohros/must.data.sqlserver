using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Nohros.Data.SqlServer.Extensions;
using Nohros.Logging;
using Nohros.Resources;
using Nohros.Extensions;

namespace Nohros.Data.SqlServer
{
  public class GetStateQuery
  {
    const string kClassName = "Nohros.Data.SqlServer.GetStateQuery";

    readonly MustLogger logger_;
    readonly SqlConnectionProvider sql_connection_provider_;

    public GetStateQuery(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
      SupressTransactions = true;
    }

    public bool Execute<T>(string state_name, string table_name, out T state,
      bool remove = false) {
      using (var scope =
        new TransactionScope(SupressTransactions
          ? TransactionScopeOption.Suppress
          : TransactionScopeOption.Required)) {
        using (SqlConnection conn = sql_connection_provider_.CreateConnection())
        using (var builder = new CommandBuilder(conn)) {
          IDbCommand cmd = builder
            .SetText(GetText(table_name, false, remove, false))
            .SetType(CommandType.Text)
            .AddParameter("@name", state_name)
            .Build();
          try {
            conn.Open();
            object obj = cmd.ExecuteScalar();
            if (obj == null) {
              state = default(T);
              scope.Complete();
              return false;
            }
            state = (T) obj;
            scope.Complete();
            return true;
          } catch (SqlException e) {
            logger_.Error(
              StringResources.Log_MethodThrowsException.Fmt("Execute",
                kClassName),
              e);
            throw e.AsProviderException();
          }
        }
      }
    }

    public IEnumerable<T> Execute<T>(string state_name, string table_name,
      bool remove = false) {
      return Execute<T>(state_name, table_name, -1, remove);
    }

    public IEnumerable<T> Execute<T>(string state_name, string table_name,
      int limit = -1, bool remove = false) {
      using (var scope =
        new TransactionScope(SupressTransactions
          ? TransactionScopeOption.Suppress
          : TransactionScopeOption.Required)) {
        using (SqlConnection conn = sql_connection_provider_.CreateConnection())
        using (var builder = new CommandBuilder(conn)) {
          IDbCommand cmd = builder
            .SetText(GetText(table_name, true, remove, limit > 0))
            .SetType(CommandType.Text)
            .AddParameter("@name", state_name)
            .Set(x => {
              if (limit > 0) {
                x.AddParameter("@limite", limit);
              }
            })
            .Build();
          try {
            conn.Open();
            var list = new List<T>();
            using (IDataReader reader = cmd.ExecuteReader()) {
              while (reader.Read()) {
                list.Add((T) reader.GetValue(0));
              }
            }
            scope.Complete();
            return list;
          } catch (SqlException e) {
            logger_.Error(
              StringResources.Log_MethodThrowsException.Fmt("Execute",
                kClassName),
              e);
            throw e.AsProviderException();
          }
        }
      }
    }

    string GetText(string table_name, bool likely, bool remove, bool limit) {
      return remove
        ? "delete " + (limit ? "top(@limite)" : "")
          + " from " + table_name
          + " output deleted.state"
          + " where state_name " + (likely ? "like" : "=") + " @name"
        : "select " + (limit ? "top(@limite)" : "") + " state "
          + "from " + table_name
          + " where state_name " + (likely ? "like" : "=") + " @name";
    }

    public bool SupressTransactions { get; set; }
  }
}
