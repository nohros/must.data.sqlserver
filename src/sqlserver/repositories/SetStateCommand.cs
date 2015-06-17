using System;
using System.Data;
using System.Data.SqlClient;
using Nohros.Logging;
using R = Nohros.Resources.StringResources;

namespace Nohros.Data.SqlServer
{
  internal class SetStateCommand
  {
    const string kClassName = "Nohros.Data.SqlServer.SetStateCommand";

    const string kExecute = SqlStateDao.kSetStateProc;
    const string kNameParameter = SqlStateDao.kStateNameParameter;
    const string kStateParameter = SqlStateDao.kStateParameter;

    readonly MustLogger logger_ = MustLogger.ForCurrentProcess;
    readonly SqlConnectionProvider sql_connection_provider_;

    #region .ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="SetStateCommand"/>
    /// using the given <param ref="sql_connection_provider" />
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> object that can be used to
    /// create connections to the data provider.
    /// </param>
    public SetStateCommand(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
      logger_ = MustLogger.ForCurrentProcess;
    }
    #endregion

    /// <inheritdoc/>
    public void Execute(string name, string state) {
      using (SqlConnection conn = sql_connection_provider_.CreateConnection())
      using (var builder = new CommandBuilder(conn)) {
        IDbCommand cmd = builder
          .SetText(sql_connection_provider_.Schema + kExecute)
          .SetType(CommandType.StoredProcedure)
          .AddParameter(kNameParameter, name)
          .AddParameter(kStateParameter, state)
          .Build();
        try {
          conn.Open();
          cmd.ExecuteNonQuery();
        } catch (SqlException e) {
          logger_.Error(
            string.Format(R.Log_MethodThrowsException, "Execute", kClassName), e);
          throw new ProviderException(e);
        }
      }
    }
  }
}
