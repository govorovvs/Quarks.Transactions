using System;
using Quarks.Transactions.Context;

namespace Quarks.Transactions
{
    public class TransactionOptions
    {
        private ITransactionContext _context = TransactionContext.CreateDefault();

        public ITransactionContext Context
        {
            get { return _context; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Context), "Transaction context must be specified");
                _context = value;
            }
        }
    }
}