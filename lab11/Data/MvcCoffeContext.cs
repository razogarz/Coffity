using lab10.Models;

namespace lab10.Data; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class MvcCoffeContext : DbContext {
    
    public MvcCoffeContext (DbContextOptions<MvcCoffeContext> options)
        : base(options)
    {
    }

    public DbSet<coffe> coffe { get; set; } = default!;
}

