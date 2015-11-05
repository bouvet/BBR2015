using System.Transactions;

namespace Database
{
    public class NoLock
    {
        public static TransactionScope CreateScope()
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions {IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted});
        }
    }
}