using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Nohros.Data.Providers;
using Nohros.Extensions;
using Nohros.Resources;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// A implementation of the <see cref="IConnectionProviderFactory"/> that
  /// create instances of the <see cref="SqlConnection"/> class.
  /// </summary>
  public class SqlConnectionProviderFactory : IConnectionProviderFactory
  {
    /// <summary>
    /// The key that should be associated with the option that contains
    /// the connection string.
    /// </summary>
    /// <remarks>
    /// This option is mutually exclulsive with the others and has the hightest
    /// priority.
    /// </remarks>
    public const string kConnectionStringOption = "connectionString";

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the Sql Server address.
    /// </summary>
    public const string kServerOption = "server";

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the login to be used to connect to the Sql Server.
    /// </summary>
    const string kLoginOption = "login";

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the login to be used to connect to the Sql Server.
    /// </summary>
    public const string kUserNameOption = "username";

    /// <summary>
    /// The key that should be associated with the option that contains
    /// the password to use to connects to the Sql Server.
    /// </summary>
    public const string kPasswordOption = "password";

    /// <summary>
    /// The key that should be associated with the options that contains the
    /// initial catalog
    /// </summary>
    public const string kInitialCatalogOption = "database";

    /// <inheritdoc/>
    public IConnectionProvider CreateProvider(
      IDictionary<string, string> options) {
      string connection_string;
      if (options.TryGetValue(kConnectionStringOption, out connection_string)) {
        return new SqlConnectionProvider(connection_string);
      }

      SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
      builder.DataSource = GetOption(kServerOption, options);

      // We try to get the user name information using the "login" key for
      // backward compatibility.
      string user_id;
      if (!options.TryGetValue(kLoginOption, out user_id)) {
        user_id = GetOption(kUserNameOption, options);
      }

      builder.UserID = user_id;
      builder.Password = GetOption(kPasswordOption, options);

      string catalog;
      if (options.TryGetValue(kInitialCatalogOption, out catalog)) {
        builder.InitialCatalog = catalog;
      }
      return new SqlConnectionProvider(builder.ConnectionString);
    }

    string GetOption(string name, IDictionary<string, string> options) {
      string option;
      if (!options.TryGetValue(name, out option)) {
        throw new KeyNotFoundException(
          string.Format(StringResources.Arg_KeyNotFound, name));
      }
      return option;
    }
  }
}
