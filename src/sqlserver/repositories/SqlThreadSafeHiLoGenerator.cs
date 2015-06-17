using System;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// An thread0safe implementation of the <see cref="IHiLoGenerator"/> class
  /// that uses the SQL Server engine to store the hi/lo values.
  /// </summary>
  public class SqlThreadSafeHiLoGenerator : ThreadSafeHiLoGenerator
  {
    #region .ctor
    public SqlThreadSafeHiLoGenerator(SqlHiLoDao sql_hi_lo_dao)
      : base(sql_hi_lo_dao.GetNextHi) {
    }
    #endregion
  }
}
