using System;
using System.Data;
using System.Data.SqlClient;
using Nohros.Data.Providers;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// A implementation of the <see cref="IConnectionProvider"/> that
  /// provides connections for the Microsoft Sql Server.
  /// </summary>
  public class SqlConnectionProvider : IConnectionProvider
  {
    const string kDefaultSchema = "dbo";
    const string kClassName = "Nohros.Data.Providers.SqlConnectionProvider";

    readonly string connection_string_;
    readonly string schema_;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SqlConnectionProvider"/> class using the specified
    /// connection string.
    /// </summary>
    public SqlConnectionProvider(string connection_string)
      : this(connection_string, kDefaultSchema) {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SqlConnectionProvider"/> class using the specified
    /// connection string and database schema.
    /// </summary>
    public SqlConnectionProvider(string connection_string, string schema) {
      connection_string_ = connection_string;
      schema_ = schema;
    }

    /// <inheritdoc/>
    IDbConnection IConnectionProvider.CreateConnection() {
      return CreateConnection();
    }

    /// <summary>
    /// Gets the schema to be used by the connection created by this class.
    /// </summary>
    /// <remarks>
    /// If a schema is not specified, this methos will return the string "dbo".
    /// </remarks>
    public string Schema {
      get { return schema_; }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SqlConnection"/> class using
    /// the provider connection string.
    /// </summary>
    /// <returns>
    /// A instance of the <see cref="SqlConnection"/> class.
    /// </returns>
    /// <remarks>
    /// If a <see cref="ITransactionContext"/> exists this, the connection
    /// that is associated with it will be returned.
    /// </remarks>
    public SqlConnection CreateConnection() {
      return new SqlConnection(connection_string_);
    }

    /// <summary>
    /// Gets the associated connection string.
    /// </summary>
    /// <value>
    /// The string that is used to connect to a SQLCE database.
    /// </value>
    public string ConnectionString {
      get { return connection_string_; }
    }
  }
}
