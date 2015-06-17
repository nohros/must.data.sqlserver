using System;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// An implementation of the <see cref="IStateDao"/> class for the SQL Server
  /// database engine.
  /// </summary>
  public class SqlStateDao : AbstractStateDao
  {
    /// <summary>
    /// The name of the procedure that will be used to persist the state.
    /// </summary>
    public const string kSetStateProc = ".nohros_state_set";

    /// <summary>
    /// The name of the stored procesdure that is used to query for the state.
    /// </summary>
    public const string kGetStateProc = ".nohros_state_get";

    /// <summary>
    /// The name of the paramter that contains the name of the state.
    /// </summary>
    public const string kStateNameParameter = "@name";

    /// <summary>
    /// The name of the parameter that contains the state value.
    /// </summary>
    public const string kStateParameter = "@state";

    readonly SqlConnectionProvider sql_connection_provider_;

    #region .ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlStateDao"/> class by
    /// using the given <see cref="SqlConnectionProvider"/> object.
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> object that can be used to
    /// access a SQL Server database.
    /// </param>
    public SqlStateDao(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
    }
    #endregion

    /// <inheritdoc/>
    public override void SetState(string name, string state) {
      new SetStateCommand(sql_connection_provider_).Execute(name, state);
    }

    /// <inheritdoc/>
    public override bool StateByName(string name, out string state) {
      return new StateByNameQuery(sql_connection_provider_)
        .Execute(name, out state);
    }
  }
}
