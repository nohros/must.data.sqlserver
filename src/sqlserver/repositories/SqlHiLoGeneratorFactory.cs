using System;
using System.Collections.Generic;
using Nohros.Configuration;
using Nohros.Providers;

namespace Nohros.Data.SqlServer
{
  public class SqlHiLoGeneratorFactory : IHiLoGeneratorFactory,
    IProviderFactory<IHiLoGenerator>

  {
    /// <summary>
    /// The key that should be associated with the option that contains
    /// the connection string.
    /// </summary>
    /// <remarks>
    /// This option is mutually exclulsive with the others and has the hightest
    /// priority.
    /// </remarks>
    public const string kConnectionStringOption =
      SqlConnectionProviderFactory.kConnectionStringOption;

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the Sql Server address.
    /// </summary>
    public const string kServerOption =
      SqlConnectionProviderFactory.kServerOption;

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the login to be used to connect to the Sql Server.
    /// </summary>
    public const string kUserNameOption =
      SqlConnectionProviderFactory.kUserNameOption;

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the password to use to connects to the Sql Server.
    /// </summary>
    public const string kPasswordOption =
      SqlConnectionProviderFactory.kPasswordOption;

    /// <summary>
    /// The key that should be associated with the option that contain the
    /// initial catalog.
    /// </summary>
    public const string kInitialCatalogOption =
      SqlConnectionProviderFactory.kInitialCatalogOption;

    /// <summary>
    /// The key that should be associated with the option that contain a
    /// value indicating if the created generator should be thread safe.
    /// </summary>
    public const string kThreadSafe = "threadSafe";

    /// <inheritdoc/>
    object IProviderFactory.CreateProvider(IDictionary<string, string> options) {
      return CreateProvider(options);
    }
    /// <inheritdoc/>
    public IHiLoGenerator CreateProvider(IDictionary<string, string> options) {
      return CreateHiLoGenerator(options);
    }

    /// <inheritdoc/>
    public IHiLoGenerator CreateHiLoGenerator(
      IDictionary<string, string> options) {
      SqlHiLoDao dao = CreateSqlDao(options);
      bool thread_safe = ProviderOptions.TryGetBoolean(options, kThreadSafe,
        false);

      return (thread_safe)
        ? new SqlThreadSafeHiLoGenerator(dao)
        : new SqlHiLoGenerator(dao) as IHiLoGenerator;
    }

    SqlHiLoDao CreateSqlDao(IDictionary<string, string> options) {
      var factory = new SqlConnectionProviderFactory();
      var sql_connection_provider = factory
        .CreateProvider(options) as SqlConnectionProvider;
      return new SqlHiLoDao(sql_connection_provider);
    }
  }
}
