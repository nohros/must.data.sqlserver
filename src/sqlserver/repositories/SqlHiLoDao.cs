using System;
using System.Transactions;

namespace Nohros.Data.SqlServer
{
  public class SqlHiLoDao : IHiLoDao
  {
    public const string kNextHiProc = ".nohros_hilo_get_next_hi";
    public const string kKeyParameter = "@key";
    public const string kCurrentHiField = "hilo_current_hi";
    public const string kMaxLoField = "hilo_max_lo";
    readonly SqlConnectionProvider sql_connection_provider_;

    #region .ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlHiLoDao"/> class
    /// by using the given <see cref="SqlConnectionProvider"/> class.
    /// </summary>
    /// <param name="sql_connection_provider">
    /// A <see cref="SqlConnectionProvider"/> object that can be used to
    /// access the SQL Server engine where the HiLo values are stored.
    /// </param>
    public SqlHiLoDao(SqlConnectionProvider sql_connection_provider) {
      sql_connection_provider_ = sql_connection_provider;
    }
    #endregion

    /// <inheritdoc/>
    public IHiLoRange GetNextHi(string key) {
      // The acquired hi should be discarded if we are inside a transaction
      // scope and it fails.
      using (new TransactionScope(TransactionScopeOption.Suppress)) {
        return new NextHiQuery(sql_connection_provider_).Execute(key);
      }
    }
  }
}
