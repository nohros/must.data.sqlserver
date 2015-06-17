using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using Nohros.Data.SqlServer.Extensions;
using Nohros.Extensions;
using Nohros.Logging;
using Nohros.Resources;
using System.Linq;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// Execute SQL commands againts a SQL Server database.
  /// </summary>
  public class SqlQueryExecutor : IQueryExecutor
  {
    const string kClassName = "Nohros.Data.SqlServer.SqlQueryExecutor";

    readonly SqlConnectionProvider sql_connection_provider_;
    readonly CommandType default_command_type_;
    readonly MustLogger logger_;

    readonly ConcurrentDictionary<string, object> cache_;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlQueryExecutor"/> class
    /// by using the given sql connection provider and default command type.
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> that can be used to create
    /// connections for a sql server.
    /// </param>
    /// <param name="default_command_type">
    /// The <see cref="CommandType"/> to be used when executing a
    /// method that does not contain a parameter of that type.
    /// </param>
    public SqlQueryExecutor(SqlConnectionProvider sql_connection_provider,
      CommandType default_command_type = CommandType.Text) {
      sql_connection_provider_ = sql_connection_provider;
      default_command_type_ = default_command_type;
      logger_ = MustLogger.ForCurrentProcess;
      cache_ = new ConcurrentDictionary<string, object>();
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// runs the method <paramref name="mapper"/> to map the result set to
    /// the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{T, TResult}"/> that maps the <see cref="IDataReader"/>
    /// created with the result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    [Obsolete(
      "This method is obsolete. Check the ExecuteQuery that returns a IQueryMapper"
      )]
    public T ExecuteQuery<T>(string query, Func<IDataReader, T> mapper) {
      return ExecuteQuery(query, mapper, builder => { },
        default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// runs the method <paramref name="mapper"/> to map the result set to
    /// the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{T, TResult}"/> that maps the <see cref="IDataReader"/>
    /// created with the result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    [Obsolete(
      "This method is obsolete. Check the ExecuteQuery that returns a IQueryMapper"
      )]
    public T ExecuteQuery<T>(string query,
      Func<IDataReader, T> mapper, CommandType command_type) {
      return ExecuteQuery(query, mapper, builder => { }, command_type);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// runs the method <paramref name="mapper"/> to map the result set to
    /// the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{T, TResult}"/> that maps the <see cref="IDataReader"/>
    /// created with the result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    [Obsolete(
      "This method is obsolete. Check the ExecuteQuery that returns a IQueryMapper"
      )]
    public T ExecuteQuery<T>(string query, Func<IDataReader, T> mapper,
      Action<CommandBuilder> set_parameters) {
      return ExecuteQuery(query, mapper, set_parameters, default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// runs the method <paramref name="mapper"/> to map the result set to
    /// the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{T, TResult}"/> that maps the <see cref="IDataReader"/>
    /// created with the result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    [Obsolete(
      "This method is obsolete. Check the ExecuteQuery that returns a IQueryMapper"
      )]
    public T ExecuteQuery<T>(string query, Func<IDataReader, T> mapper,
      Action<CommandBuilder> set_parameters, CommandType command_type) {
      using (SqlConnection conn = sql_connection_provider_.CreateConnection())
      using (var builder = new CommandBuilder(conn)) {
        IDbCommand cmd = builder
          .SetText(query)
          .SetType(command_type)
          .Set(set_parameters)
          .Build();
        try {
          conn.Open();
          using (IDataReader reader = cmd.ExecuteReader()) {
            return mapper(reader);
          }
        } catch (SqlException e) {
          logger_.Error(
            StringResources
              .Log_MethodThrowsException
              .Fmt("ExecuteQuery", kClassName), e);
          throw e.AsProviderException();
        }
      }
    }

    /// <inheritdoc/>
    public IQueryMapper<T> ExecuteQuery<T>(string query) {
      IDataReaderMapper<T> mapper =
        new DataReaderMapperBuilder<T>()
          .AutoMap()
          .Build();
      return ExecuteQuery(query, () => mapper);
    }

    /// <inheritdoc/>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      CommandType command_type) {
      IDataReaderMapper<T> mapper =
        new DataReaderMapperBuilder<T>()
          .AutoMap()
          .Build();
      return ExecuteQuery(query, () => mapper, command_type);
    }

    /// <inheritdoc/>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      Action<CommandBuilder> set_parameters) {
      IDataReaderMapper<T> mapper =
        new DataReaderMapperBuilder<T>()
          .AutoMap()
          .Build();
      return ExecuteQuery(query, () => mapper, set_parameters);
    }

    /// <inheritdoc/>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      Action<CommandBuilder> set_parameters,
      CommandType command_type) {
      IDataReaderMapper<T> mapper =
        new DataReaderMapperBuilder<T>()
          .AutoMap()
          .Build();
      return ExecuteQuery(query, () => mapper, set_parameters, command_type);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns a <see cref="IDataReaderMapper{T}"/> that can be used to
    /// map the result set to a object of the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{TResult}"/> that builds a
    /// <see cref="IDataReaderMapper{T}"/> that can be used to map
    /// result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    /// <remarks>
    /// The mapper returned from the <see cref="Func{TResult}"/> is cached
    /// internally and associated with the given <paramref name="query"/>. The
    /// cache is never flushed. If you are generating SQL strings on the fly
    /// without using parameters it is possible you hit memory issues.
    /// </remarks>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      Func<IDataReaderMapper<T>> mapper) {
      return ExecuteQuery(query, mapper, builder => { },
        default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns a <see cref="IDataReaderMapper{T}"/> that can be used to
    /// map the result set to a object of the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{TResult}"/> that builds a
    /// <see cref="IDataReaderMapper{T}"/> that can be used to map
    /// result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    /// <remarks>
    /// The mapper returned from the <see cref="Func{TResult}"/> is cached
    /// internally and associated with the given <paramref name="query"/>. The
    /// cache is never flushed. If you are generating SQL strings on the fly
    /// without using parameters it is possible you hit memory issues.
    /// </remarks>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      Func<IDataReaderMapper<T>> mapper, CommandType command_type) {
      return ExecuteQuery(query, mapper, builder => { }, command_type);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns a <see cref="IDataReaderMapper{T}"/> that can be used to
    /// map the result set to a object of the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{TResult}"/> that builds a
    /// <see cref="IDataReaderMapper{T}"/> that can be used to map
    /// result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    /// <remarks>
    /// The mapper returned from the <see cref="Func{TResult}"/> is cached
    /// internally and associated with the given <paramref name="query"/>. The
    /// cache is never flushed. If you are generating SQL strings on the fly
    /// without using parameters it is possible you hit memory issues.
    /// </remarks>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      Func<IDataReaderMapper<T>> mapper,
      Action<CommandBuilder> set_parameters) {
      return ExecuteQuery(query, mapper, set_parameters, default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// runs the method <paramref name="mapper"/> to map the result set to
    /// the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that should be returned.
    /// </typeparam>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <param name="mapper">
    /// A <see cref="Func{TResult}"/> that builds a
    /// <see cref="IDataReaderMapper{T}"/> that can be used to map
    /// result of the given <paramref name="query"/> to a
    /// object of the type <typeparamref name="T"/>.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The object produced by the execution of the method
    /// <paramref name="mapper"/>.
    /// </returns>
    /// <remarks>
    /// The mapper returned from the <see cref="Func{TResult}"/> is cached
    /// internally and associated with the given <paramref name="query"/>. The
    /// cache is never flushed. If you are generating SQL strings on the fly
    /// without using parameters it is possible you hit memory issues.
    /// </remarks>
    public IQueryMapper<T> ExecuteQuery<T>(string query,
      Func<IDataReaderMapper<T>> mapper,
      Action<CommandBuilder> set_parameters,
      CommandType command_type) {
      SqlConnection conn = sql_connection_provider_.CreateConnection();
      var builder = new CommandBuilder(conn);
      IDbCommand cmd = builder
        .SetText(query)
        .SetType(command_type)
        .Set(set_parameters)
        .Build();
      try {
        conn.Open();
        IDataReader reader = cmd.ExecuteReader();
        var m = (IDataReaderMapper<T>) cache_.GetOrAdd(query, s => mapper());
        return new QueryMapper<T>(m, reader, new IDisposable[] {builder, conn});
      } catch (SqlException e) {
        logger_.Error(
          StringResources
            .Log_MethodThrowsException
            .Fmt("ExecuteQuery", kClassName), e);
        builder.Dispose();
        conn.Dispose();
        throw e.AsProviderException();
      } catch {
        builder.Dispose();
        conn.Dispose();
        throw;
      }
    }


    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns the number of rows affected.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int ExecuteNonQuery(string query) {
      return ExecuteNonQuery(query, builder => { }, default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns the number of rows affected.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int ExecuteNonQuery(string query, CommandType command_type) {
      return ExecuteNonQuery(query, builder => { }, command_type);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns the number of rows affected.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int ExecuteNonQuery(string query,
      Action<CommandBuilder> set_parameters) {
      return ExecuteNonQuery(query, set_parameters, default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returns the number of rows affected.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int ExecuteNonQuery(string query,
      Action<CommandBuilder> set_parameters, CommandType command_type) {
      using (SqlConnection conn = sql_connection_provider_.CreateConnection())
      using (var builder = new CommandBuilder(conn)) {
        IDbCommand cmd = builder
          .SetText(query)
          .SetType(command_type)
          .Set(set_parameters)
          .Build();
        try {
          conn.Open();
          return cmd.ExecuteNonQuery();
        } catch (SqlException e) {
          logger_.Error(
            StringResources
              .Log_MethodThrowsException
              .Fmt("ExecuteNonQuery", kClassName), e);
          throw e.AsProviderException();
        }
      }
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <returns>
    /// The value of the first column of the first row of the recordset
    /// resulted from the execution of the <paramref name="query"/>.
    /// </returns>
    public T ExecuteScalar<T>(string query) {
      return ExecuteScalar<T>(query, builder => { }, default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="t">
    /// When this method returns contains the value of the first column of
    /// the first row of the recordset resulted from the execution of the
    /// <paramref name="query"/> if the recordset is not empty; otherwise,
    /// the default value for the type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the recordset resulted from the execution of the
    /// <paramref name="query"/> is not empty; otherwise, <c>false</c>.
    /// </returns>
    public bool ExecuteScalar<T>(string query, out T t) {
      return ExecuteScalar(query, builder => { }, default_command_type_, out t);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <returns>
    /// The value of the first column of the first row of the recordset
    /// resulted from the execution of the <paramref name="query"/>.
    /// </returns>
    public T ExecuteScalar<T>(string query, CommandType command_type) {
      return ExecuteScalar<T>(query, builder => { }, command_type);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <param name="t">
    /// When this method returns contains the value of the first column of
    /// the first row of the recordset resulted from the execution of the
    /// <paramref name="query"/> if the recordset is not empty; otherwise,
    /// the default value for the type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the recordset resulted from the execution of the
    /// <paramref name="query"/> is not empty; otherwise, <c>false</c>.
    /// </returns>
    public bool ExecuteScalar<T>(string query, CommandType command_type, out T t) {
      return ExecuteScalar(query, builder => { }, command_type, out t);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The value of the first column of the first row of the recordset
    /// resulted from the execution of the <paramref name="query"/>.
    /// </returns>
    public T ExecuteScalar<T>(string query,
      Action<CommandBuilder> set_parameters) {
      return ExecuteScalar<T>(query, set_parameters, default_command_type_);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <param name="t">
    /// When this method returns contains the value of the first column of
    /// the first row of the recordset resulted from the execution of the
    /// <paramref name="query"/> if the recordset is not empty; otherwise,
    /// the default value for the type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the recordset resulted from the execution of the
    /// <paramref name="query"/> is not empty; otherwise, <c>false</c>.
    /// </returns>
    public bool ExecuteScalar<T>(string query,
      Action<CommandBuilder> set_parameters, out T t) {
      return ExecuteScalar(query, set_parameters, default_command_type_, out t);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <returns>
    /// The value of the first column of the first row of the recordset
    /// resulted from the execution of the <paramref name="query"/>.
    /// </returns>
    public T ExecuteScalar<T>(string query,
      Action<CommandBuilder> set_parameters, CommandType command_type) {
      T t;
      if (ExecuteScalar(query, set_parameters, command_type, out t)) {
        return t;
      }
      return default(T);
    }

    /// <summary>
    /// Executes the command described by <see cref="query"/> on the server and
    /// returned the value of the first column of the first row of the
    /// resulting recordset.
    /// </summary>
    /// <param name="query">
    /// The query to be executed on the server.
    /// </param>
    /// <param name="command_type">
    /// The type of the command that is described by the
    /// <paramref name="query"/> parameter.
    /// </param>
    /// <param name="set_parameters">
    /// A <see cref="Action{T}"/> that allows the caller to set the values
    /// of the parameters defined on the given query.
    /// </param>
    /// <param name="t">
    /// When this method returns contains the value of the first column of
    /// the first row of the recordset resulted from the execution of the
    /// <paramref name="query"/> if the recordset is not empty; otherwise,
    /// the default value for the type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the recordset resulted from the execution of the
    /// <paramref name="query"/> is not empty; otherwise, <c>false</c>.
    /// </returns>
    public bool ExecuteScalar<T>(string query,
      Action<CommandBuilder> set_parameters, CommandType command_type, out T t) {
      using (SqlConnection conn = sql_connection_provider_.CreateConnection())
      using (var builder = new CommandBuilder(conn)) {
        IDbCommand cmd = builder
          .SetText(query)
          .SetType(command_type)
          .Set(set_parameters)
          .Build();
        try {
          conn.Open();
          object obj = cmd.ExecuteScalar();
          if (obj != null) {
            t = (T) obj;
            return true;
          }
          t = default(T);
          return false;
        } catch (SqlException e) {
          logger_.Error(
            StringResources
              .Log_MethodThrowsException
              .Fmt("ExecuteNonQuery", kClassName), e);
          throw e.AsProviderException();
        }
      }
    }
  }
}
