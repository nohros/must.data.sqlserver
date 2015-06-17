using System;
using System.Data;
using System.Data.SqlClient;
using Nohros.Logging;
using R = Nohros.Resources.StringResources;

namespace Nohros.Data.SqlServer
{
  public class StateByNameQuery
  {
    const string kClassName = "Nohros.Data.SqlServer.StateByNameQuery";

    const string kExecute = SqlStateDao.kGetStateProc;
    const string kNameParameter = SqlStateDao.kStateNameParameter;

    readonly MustLogger logger_ = MustLogger.ForCurrentProcess;
    readonly SqlConnectionProvider sql_connection_provider_;

    #region .ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="StateByNameQuery"/>
    /// using the given <param ref="sql_connection_provider" />
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> object that can be used to
    /// create connections to the data provider.
    /// </param>
    public StateByNameQuery(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
    }
    #endregion

    /// <exception cref="ProviderException">
    /// An exception has occured while executing the query.
    /// </exception>
    public bool Execute(string name, out string state) {
      using (SqlConnection conn = sql_connection_provider_.CreateConnection())
      using (var builder = new CommandBuilder(conn)) {
        IDbCommand cmd = builder
          .SetText(sql_connection_provider_.Schema + kExecute)
          .SetType(CommandType.StoredProcedure)
          .AddParameter(kNameParameter, name)
          .Build();
        try {
          conn.Open();
          object obj = cmd.ExecuteScalar();
          if (obj != null) {
            state = (string) obj;
            return true;
          }
        } catch (SqlException e) {
          logger_.Error(string.Format(
            R.Log_MethodThrowsException, "Execute", kClassName), e);
          throw new ProviderException(e);
        }
        state = null;
        return false;
      }
    }
  }
}
