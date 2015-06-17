using System;
using System.Collections.Generic;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// An implementation of the <see cref="IStateDaoFactory"/> class for the
  /// SQL Server.
  /// </summary>
  public class SqlDaoFactory : AbstractSqlDaoFactory<IStateDao>,
                               IStateDaoFactory
  {
    public IStateDao CreateStateDao(IDictionary<string, string> options) {
      return CreateProvider(options, provider => new SqlStateDao(provider));
    }
  }
}
