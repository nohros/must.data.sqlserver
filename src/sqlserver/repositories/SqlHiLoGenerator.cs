using System;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// An implementation of the <see cref="IHiLoGenerator"/> class that uses
  /// the SQL Server engine to store the hi/lo values.
  /// </summary>
  public class SqlHiLoGenerator : HiLoGenerator
  {
    #region .ctor
    public SqlHiLoGenerator(SqlHiLoDao sql_hi_lo_dao)
      : base(sql_hi_lo_dao.GetNextHi) {
    }
    #endregion
  }
}
