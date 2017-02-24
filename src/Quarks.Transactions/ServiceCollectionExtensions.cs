using System;
using Microsoft.Extensions.DependencyInjection;
using Quarks.Transactions.Context;
using Quarks.Transactions.Impl;

namespace Quarks.Transactions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactions(this IServiceCollection services)
        {
            return AddTransactions(services, options => { });
        }

        public static IServiceCollection AddTransactions(this IServiceCollection services, Action<TransactionOptions> setupAction)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            TransactionOptions options = CreateOptions(setupAction);

            services.AddTransient<ITransactionFactory, TransactionFactory>();

            TransactionContext.Current = options.Context;
            return services;
        }

        private static TransactionOptions CreateOptions(Action<TransactionOptions> setupAction)
        {
            var options = new TransactionOptions();
            setupAction(options);
            return options;
        }
    }
}