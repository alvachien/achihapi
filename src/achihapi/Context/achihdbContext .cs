using achihapi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace achihapi.Context
{
    public partial class achihdbContext : DbContext
    {
        public achihdbContext()
        {
        }

        public achihdbContext(DbContextOptions<achihdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<FinanceCurrencyModel> Financecurrency { get; set; }
    }
}

