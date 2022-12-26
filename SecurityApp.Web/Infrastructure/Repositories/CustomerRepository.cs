﻿using Microsoft.EntityFrameworkCore;
using SecurityApp.Web.Infrastructure.Entities.Exceptions;
using SecurityApp.Web.Infrastructure.Entities.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityApp.Web.Infrastructure.Repositories
{
    public class CustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(CustomerModel pEntity)
        {
            _context.Customer.Add(pEntity);
            if (!(await _context.SaveChangesAsync() > 0)) 
            { 
                throw new Exception("There was an error create record");
            }
        }

        public async Task<CustomerModel> ReadById(Guid pId)
        {
            return await _context.Customer.AsNoTracking().Where(e => e.Id.Equals(pId)).FirstOrDefaultAsync();
        }

        public async Task<CustomerModel> ReadByMail(string pMail)
        {
            return await _context.Customer.AsNoTracking().Where(e => e.Mail.Equals(pMail)).FirstOrDefaultAsync();
        }

        public async Task<object> Read(CustomerModel pEntity)
        {
            IQueryable<CustomerModel> query = _context.Customer.AsNoTracking();
            return new
            {
                total = query.Select(x => new { x.Id }).Count(),
                data = await query.ToListAsync(),
            };
        }

        public async Task UpdateAsync(CustomerModel pEntity)
        {
            _context.Customer.Update(pEntity);
            if (!(await _context.SaveChangesAsync() > 0))
            {
                throw new Exception("There was an error update record");
            }
        }

        public async Task DeleteAsync(CustomerModel pEntity)
        {
            _context.Customer.Remove(pEntity);
            await _context.SaveChangesAsync();
        }
    }
}