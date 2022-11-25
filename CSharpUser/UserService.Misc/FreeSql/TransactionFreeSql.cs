using System.Data;
using System.Data.Common;
using FreeSql;
using FreeSql.Internal;
using FreeSql.Internal.ObjectPool;

namespace UserService.Misc.FreeSql;

// modified from https://github.com/dotnetcore/FreeSql/issues/322
public interface ITransactionFreeSql : IFreeSql
{
    void Commit();
    void Rollback();
}

public class TransactionFreeSql : ITransactionFreeSql
{
    private readonly IFreeSql              _originalFsql;
    private          Object<DbConnection>? _connection;

    private bool           _disposed;
    private DbTransaction? _transaction;
    private int            _transactionCount;

    public TransactionFreeSql(IFreeSql fsql) => _originalFsql = fsql;

    public IAdo         Ado          => _originalFsql.Ado;
    public IAop         Aop          => _originalFsql.Aop;
    public ICodeFirst   CodeFirst    => _originalFsql.CodeFirst;
    public IDbFirst     DbFirst      => _originalFsql.DbFirst;
    public GlobalFilter GlobalFilter => _originalFsql.GlobalFilter;

    public ISelect<T1> Select<T1>() where T1 : class => _originalFsql.Select<T1>().WithTransaction(_transaction);

    public ISelect<T1> Select<T1>(object dywhere) where T1 : class => Select<T1>().WhereDynamic(dywhere);

    public IDelete<T1> Delete<T1>() where T1 : class => _originalFsql.Delete<T1>().WithTransaction(_transaction);

    public IDelete<T1> Delete<T1>(object dywhere) where T1 : class => Delete<T1>().WhereDynamic(dywhere);

    public IUpdate<T1> Update<T1>() where T1 : class => _originalFsql.Update<T1>().WithTransaction(_transaction);

    public IUpdate<T1> Update<T1>(object dywhere) where T1 : class => Update<T1>().WhereDynamic(dywhere);

    public IInsert<T1> Insert<T1>() where T1 : class => _originalFsql.Insert<T1>().WithTransaction(_transaction);

    public IInsert<T1> Insert<T1>(T1 source) where T1 : class => Insert<T1>().AppendData(source);

    public IInsert<T1> Insert<T1>(T1[] source) where T1 : class => Insert<T1>().AppendData(source);

    public IInsert<T1> Insert<T1>(List<T1> source) where T1 : class => Insert<T1>().AppendData(source);

    public IInsert<T1> Insert<T1>(IEnumerable<T1> source) where T1 : class => Insert<T1>().AppendData(source);

    public IInsertOrUpdate<T1> InsertOrUpdate<T1>() where T1 : class =>
        _originalFsql.InsertOrUpdate<T1>().WithTransaction(_transaction);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    // public void Transaction(Action handler) => TransactionPriv(null, handler);
    // Force isolation level to RC
    public void Transaction(Action handler) => TransactionPriv(IsolationLevel.ReadCommitted, handler);

    public void Transaction(IsolationLevel isolationLevel, Action handler) => TransactionPriv(isolationLevel, handler);

    public void Commit() => TransactionCommitPriv(true);

    public void Rollback() => TransactionCommitPriv(false);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // keep original fsql alive cause singleton
            // _originalFsql.Dispose();
        }

        TransactionCommitPriv(true);
        _disposed = true;
    }

    ~TransactionFreeSql() => Dispose(false);

    private void TransactionPriv(IsolationLevel? isolationLevel, Action handler)
    {
        if (_transaction != null)
        {
            _transactionCount++;
            return; // Tx already start
        }

        try
        {
            _connection ??= _originalFsql.Ado.MasterPool.Get();
            _transaction = isolationLevel == null
                ? _connection.Value.BeginTransaction()
                : _connection.Value.BeginTransaction(isolationLevel.Value);
            _transactionCount = 1;
        }
        catch
        {
            TransactionCommitPriv(false);
            throw;
        }
    }

    private void TransactionCommitPriv(bool isCommit)
    {
        if (_transaction == null)
        {
            return;
        }

        _transactionCount--;
        if (_transactionCount > 0)
        {
            return; // ignore internal tx
        }

        try
        {
            if (isCommit == false)
            {
                _transaction.Rollback();
            }
            else if (_transactionCount <= 0)
            {
                _transaction.Commit();
            }
        }
        finally
        {
            if (isCommit == false || _transactionCount <= 0)
            {
                _originalFsql.Ado.MasterPool.Return(_connection);
                _connection  = null;
                _transaction = null;
            }
        }
    }
}
