using System;
using System.Collections.Generic;
using Nohros.Extensions;
using Nohros.Providers;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// A factory for the <see cref="SqlAppState"/> class.
  /// </summary>
  public class SqlAppStateFactory : IProviderFactory<SqlAppState>
  {
    /// <summary>
    /// The key that should be associated with the option that caontains the
    /// flag that indicates if transactions should be suppressed.
    /// </summary>
    public const string kSupressTransactions = "supressTransactions";

    object IProviderFactory.CreateProvider(IDictionary<string, string> options) {
      return CreateProvider(options);
    }

    public SqlAppState CreateProvider(IDictionary<string, string> options) {
      return CreateAppState(options) as SqlAppState;
    }

    /// <summary>
    /// Creates an instance of the <see cref="IAppState"/> that uses the
    /// SQL Server engine as the state storage.
    /// </summary>
    /// <param name="options">
    /// A collection of key/value pairs containing the options used to create
    /// the app state and its associated connection string.
    /// </param>
    /// <returns>
    /// A <see cref="IAppState"/> object that uses the SQL Server engine to
    /// store the states.
    /// </returns>
    /// <see cref="kSupressTransactions"/>
    /// <see cref="SqlConnectionProvider"/>
    /// <see cref="SqlConnectionProviderFactory"/>
    /// <see cref="SqlConnectionProviderFactory.kConnectionStringOption"/>
    /// <see cref="SqlConnectionProviderFactory.kInitialCatalogOption"/>
    /// <see cref="SqlConnectionProviderFactory.kLoginOption"/>
    /// <see cref="SqlConnectionProviderFactory.kPasswordOption"/>
    /// <see cref="SqlConnectionProviderFactory.kServerOption"/>
    public IAppState CreateAppState(IDictionary<string, string> options) {
      var factory = new SqlConnectionProviderFactory();
      var sql_connection_provider = factory
        .CreateProvider(options) as SqlConnectionProvider;
      bool supress_dtc =
        bool.Parse(options.GetString(kSupressTransactions, "false"));
      return new SqlAppState(sql_connection_provider, supress_dtc);
    }
  }
}
