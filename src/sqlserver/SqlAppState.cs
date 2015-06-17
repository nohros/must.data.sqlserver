using System;
using System.Collections.Generic;

namespace Nohros.Data.SqlServer
{
  public class SqlAppState : IAppState
  {
    // Tables
    const string kBoolTableName = "nohros_state_bool";
    const string kShortTableName = "nohros_state_short";
    const string kIntTableName = "nohros_state_int";
    const string kLongTableName = "nohros_state_long";
    const string kDecimalTableName = "nohros_state_decimal";
    const string kDoubleTableName = "nohros_state_double";
    const string kStringTableName = "nohros_state_string";
    const string kGuidTableName = "nohros_state_guid";
    const string kDateTimeTableName = "nohros_state_date";

    readonly AddStateQuery add_state_;
    readonly GetStateQuery get_state_;
    readonly UpdateStateQuery update_state_;
    readonly SetIfQuery if_query_;
    readonly RemoveStateQuery remove_state_;
    readonly MergeStateQuery merge_state_;

    bool supress_dtc_;

    public SqlAppState(SqlConnectionProvider sql_connection_provider,
      bool supress_dtc = false) {
      update_state_ = new UpdateStateQuery(sql_connection_provider);
      get_state_ = new GetStateQuery(sql_connection_provider);
      add_state_ = new AddStateQuery(sql_connection_provider);
      remove_state_ = new RemoveStateQuery(sql_connection_provider);
      merge_state_ = new MergeStateQuery(sql_connection_provider);
      if_query_ = new SetIfQuery(sql_connection_provider);

      SupressTransactions = supress_dtc;
    }

    /// <inheritdoc/>
    public bool Contains<T>(string name) {
      T @out;
      return Get(name, out @out);
    }

    /// <inheritdoc/>
    public T Get<T>(string name) {
      return Get<T>(name, false);
    }

    /// <inheritdoc/>
    public T Get<T>(string name, bool remove) {
      T state;
      if (!Get(name, remove, out state)) {
        throw new NoResultException();
      }
      return state;
    }

    /// <inheritdoc/>
    public T Get<T>(string name, T def) {
      T state;
      if (!Get(name, out state)) {
        return def;
      }
      return state;
    }

    /// <inheritdoc/>
    public bool Get<T>(string name, out T state) {
      return Get(name, false, out state);
    }

    bool Get<T>(string name, bool remove, out T state) {
      return get_state_.Execute(name, GetTableNameForType<T>(), out state,
        remove);
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetForPrefix<T>(string prefix) {
      return GetForPrefix<T>(prefix, -1, false);
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetForPrefix<T>(string prefix, int limit) {
      return GetForPrefix<T>(prefix, limit, false);
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetForPrefix<T>(string prefix, bool remove) {
      return GetForPrefix<T>(prefix, -1, remove);
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetForPrefix<T>(string prefix, int limit, bool remove) {
      return get_state_.Execute<T>(prefix + '%', GetTableNameForType<T>(),
        limit, remove);
    }

    /// <inheritdoc/>
    public bool Remove<T>(string name) {
      return remove_state_.Execute(name, GetTableNameForType<T>()) != 0;
    }

    /// <inheritdoc/>
    public int RemoveForPrefix<T>(string prefix) {
      return remove_state_.Execute(prefix, GetTableNameForType<T>(), true);
    }

    /// <inheritdoc/>
    public void Set<T>(string name, T state) {
      string table_name = GetTableNameForType<T>();
      // lets try to update the value upfront and if nothing changes,
      // try to add the value to the table.
      if (!update_state_.Execute(name, table_name, state)) {
        // If a insert is performed after our update attempt and before
        // our insert attempt a unique constraint exception will be throw.
        // In that case we will try to perform the update again.
        try {
          add_state_.Execute(name, table_name, state);
        } catch (UniqueConstraintViolationException) {
          update_state_.Execute(name, table_name, state);
        }
      }
    }

    /// <inheritdoc/>
    public bool SetIf<T>(ComparisonOperator op, string name, T state,
      T comparand) {
      string table_name = GetTableNameForType<T>();
      // lets try to update the value upfront and if nothing changes,
      // check if the value exists and create a new one if not.
      if (!if_query_.Execute(op, name, table_name, state, comparand)) {
        T obj;
        if (!get_state_.Execute(name, table_name, out obj)) {
          // If a insert is performed after our update attempt and before
          // our insert attempt a unique constraint exception will be throw.
          // In that case we will try to perform the update again.
          try {
            add_state_.Execute(name, table_name, state);
            return true;
          } catch (UniqueConstraintViolationException) {
            return if_query_.Execute(op, name, table_name, state,
              comparand);
          }
        }
        return false;
      }
      return true;
    }

    /// <inheritdoc/>
    public void Merge<T>(string name, T state) {
      string table_name = GetTableNameForType<T>();
      // lets try to update the value upfront and if nothing changes,
      // check if the value exists and create a new one if not.
      if (!merge_state_.Execute(name, table_name, state)) {
        T obj;
        if (!get_state_.Execute(name, table_name, out obj)) {
          // If a insert is performed after our update attempt and before
          // our insert attempt a unique constraint exception will be throw.
          // In that case we will try to perform the update again.
          try {
            add_state_.Execute(name, table_name, state);
          } catch (UniqueConstraintViolationException) {
            merge_state_.Execute(name, table_name, state);
          }
        }
      }
    }

    string GetTableNameForType<T>() {
      string t = typeof (T).Name;
      switch (t) {
        case "Boolean":
          return kBoolTableName;

        case "Int16":
          return kShortTableName;

        case "Int32":
          return kIntTableName;

        case "Int64":
          return kLongTableName;

        case "Decimal":
          return kDecimalTableName;

        case "Double":
          return kDoubleTableName;

        case "String":
          return kStringTableName;

        case "Guid":
          return kGuidTableName;

        case "DateTime":
          return kDateTimeTableName;

        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Gets a value indicating if transactions should be supressed.
    /// </summary>
    public bool SupressTransactions {
      get { return supress_dtc_; }
      set {
        supress_dtc_ = value;
        update_state_.SupressTransactions = value;
        get_state_.SupressTransactions = value;
        add_state_.SupressTransactions = value;
        remove_state_.SupressTransactions = value;
        merge_state_.SupressTransactions = value;
        if_query_.SupressTransactions = value;
      }
    }
  }
}
