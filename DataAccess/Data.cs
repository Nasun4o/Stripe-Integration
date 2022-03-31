using DataAccess;
using Entities;
using Entities.EntityModels;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class Data : IData
    {
        private StripeIntegrationContext _stripeIntegrationContext;
        private readonly IDictionary<Type, object> repositories;

        public Data(StripeIntegrationContext stripeIntegrationContext)
        {
            _stripeIntegrationContext = stripeIntegrationContext;
            repositories = new Dictionary<Type, object>();
        }

        public IRepositoryBase<ApplicationUser> ApplicationUser => GetRepository<ApplicationUser>();

        public IRepositoryBase<StripeTransactionHistory> StripeTransactionHistories => GetRepository<StripeTransactionHistory>();

        public async Task<int> SaveAsync()
        {
            return await _stripeIntegrationContext.SaveChangesAsync();
        }

        private IRepositoryBase<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!repositories.ContainsKey(type))
            {
                var typeOfRepository = typeof(RepositoryBase<TEntity>);
                var repository = Activator.CreateInstance(typeOfRepository, _stripeIntegrationContext);

                repositories.Add(type, repository);
            }

            return (IRepositoryBase<TEntity>)repositories[type];
        }
    }
}
