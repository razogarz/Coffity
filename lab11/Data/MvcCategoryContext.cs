using lab10.Models;

namespace lab10.Data; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class MvcCategoryContext : DbContext {
    
    public MvcCategoryContext (DbContextOptions<MvcCategoryContext> options)
        : base(options)
    {
    }

    public DbSet<coffe> category { get; set; } = default!;
}

