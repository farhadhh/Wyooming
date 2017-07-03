using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Wyooming.Data;

namespace Wyooming.Models
{
    public class WyoomingContext : DbContext
    {
        public WyoomingContext (DbContextOptions<WyoomingContext> options)
            : base(options)
        {
        }

        public DbSet<Wyooming.Data.Contact> Contact { get; set; }
    }
}
