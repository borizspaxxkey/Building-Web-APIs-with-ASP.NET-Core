using H_Plus_Sports.Contracts;
using H_Plus_Sports.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H_Plus_Sports.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private H_Plus_SportsContext _context;
        private IDistributedCache _redisCache;

        public CustomerRepository(H_Plus_SportsContext context, IDistributedCache redisCache)
        {
            _context = context;
            _redisCache = redisCache;
        }
        public async Task<Customer> Add(Customer customer)
        {
            await _context.Customer.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> Exist(int id)
        {
            return await _context.Customer.AnyAsync(c => c.CustomerId == id);
        }

        public async Task<Customer> Find(int id)
        {
            // Redis Caching Technique 

            var stringId = id.ToString();
            var cachedCustomer = await _redisCache.GetStringAsync(stringId);

            if (cachedCustomer != null)
            {
                return JsonConvert.DeserializeObject<Customer>(cachedCustomer);
            }
            else
            {
                var dbCustomer = await _context.Customer.SingleOrDefaultAsync(o => o.CustomerId == id);

                await _redisCache.SetStringAsync(stringId, JsonConvert.SerializeObject(dbCustomer));

                return dbCustomer;
            }
        }

        public IEnumerable<Customer> GetAll()
        {
            return _context.Customer;
        }

        public async Task<Customer> Remove(int id)
        {
            var customer = await _context.Customer.SingleOrDefaultAsync(a => a.CustomerId == id);
            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> Update(Customer customer)
        {
            _context.Customer.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }
    }
}
