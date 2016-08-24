using System.Threading.Tasks;

namespace Quarks.Transactions.Impl
{
	public static class TaskEx
	{
		public static Task CompletedTask
		{
			get
			{
#if NET_45
				return Task.FromResult(0);
#else
				return Task.CompletedTask;
#endif
			}
		}
	}
}