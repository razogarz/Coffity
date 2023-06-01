using lab10.Models;

namespace lab10.Data; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class MvcRecipeContext : DbContext {
    
    public MvcRecipeContext (DbContextOptions<MvcRecipeContext> options)
        : base(options)
    {
    }

    public DbSet<coffe> recipe { get; set; } = default!;
}

