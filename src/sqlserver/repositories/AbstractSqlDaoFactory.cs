using System;
using System.Collections.Generic;
using Nohros.Providers;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// An abstract implementation of the <see cref="IProviderFactory{T}"/>
  /// interface that can be used to reduce the effort required to create
  /// instances of classes that have a constructor that receives a
  /// <see cref="SqlConnectionProvider"/> object as one of its argument.
  /// </summary>
  /// <remarks>
  /// The <see cref="SqlConnectionProvider"/> object will be created using
  /// the information contained on the given options object.
  /// </remarks>
  /// <typeparam name="T">
  /// A class that has a constructor that receives a
  /// <see cref="SqlConnectionProvider"/> as argument.
  /// </typeparam>
  public class AbstractSqlDaoFactory<T> : IProviderFactory<T> where T : class
  {
    /// <summary>
    /// The method that is used to create an instance of the type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// <param name="sql_connection_provider">
    /// The <see cref="SqlConnectionProvider"/> object that was created using
    /// the information contained on a given collection of options.
    /// </param>
    /// <returns>
    /// The newly created <typeparamref name="T"/> object.
    /// </returns>
    protected delegate T SqlDaoFactoryDelegate(
      SqlConnectionProvider sql_connection_provider);

    /// <inheritdoc/>
    /// <remarks>
    /// The <typeparamref name="T"/> object should have a constructor that
    /// receives a single parameter of the type
    /// <see cref="SqlConnectionProvider"/>.
    /// </remarks>
    public T CreateProvider(IDictionary<string, string> options) {
      return CreateProvider(options,
        sql_connection_provider =>
          RuntimeTypeFactory<T>
            .CreateInstance(typeof (T), sql_connection_provider));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The type of the provider to be created should have a constructor that
    /// receives a single bparameter of the type
    /// <see cref="SqlConnectionProvider"/>.
    /// </remarks>
    object IProviderFactory.CreateProvider(IDictionary<string, string> options) {
      return CreateProvider(options);
    }

    /// <summary>
    /// Create a object of type <typeparamref name="T"/> using the given
    /// collection of options and factory.
    /// </summary>
    /// <param name="options">
    /// A collection of key/value pairs containing the keys that is required to
    /// create an instance of the <see cref="SqlConnectionProvider"/> class.
    /// </param>
    /// <param name="factory">
    /// The method taht receives a <see cref="SqlConnectionProvider"/> and
    /// returns an instance of the tyep <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The newly created object.
    /// </returns>
    protected T CreateProvider(IDictionary<string, string> options,
      SqlDaoFactoryDelegate factory) {
      var sql_connection_provider =
        new SqlConnectionProviderFactory()
          .CreateProvider(options) as SqlConnectionProvider;
      return factory(sql_connection_provider);
    }
  }
}
