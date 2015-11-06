using System.Transactions;
using Database.Infrastructure;

namespace Database
{
    public class With
    {
        public static TransactionScope ReadUncommitted()
        {
            var settings = ServiceLocator.Current.Resolve<OverridableSettings>();

            if (settings.KjørReadUncommitted)
            {
                return new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted});
            }

            return new TransactionScope();
        }
    }
}