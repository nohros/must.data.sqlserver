using System;
using System.Data.SqlClient;

namespace Nohros.Data.SqlServer.Extensions
{
  public static class SqlExceptionExtensions
  {
    public static ProviderException AsProviderException(
      this SqlException exception) {
      if (exception.State != 0) {
        switch (exception.Number) {
          case 2601: // duplicate index constraint
          case 2627: // duplicate check constraint
            return new UniqueConstraintViolationException(exception);
          case 515: // insert on non nulable column
            return new NotNullConstraintViolationException(exception);
        }

        if (exception.Number >= 50000) {
          return new CustomProviderException(exception);
        }
      }
      return new ProviderException(exception);
    }
  }
}
