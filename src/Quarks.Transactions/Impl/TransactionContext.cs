namespace Quarks.Transactions.Impl
{
	internal static class TransactionContext
	{
		private static readonly object Lock = new object();
		private static volatile ITransactionContext _current;

		public static ITransactionContext Current
		{
			get
			{
				if (_current == null)
				{
					lock (Lock)
					{
						if (_current == null)
						{
							_current = CreateContext();
						}
					}
				}

				return _current;
			}
		}

		private static ITransactionContext CreateContext()
		{
#if NET_45
			return new CallContextTransactionContext();
#else
			return new AsyncLocalTransactionContext();
#endif
		}
	}
}