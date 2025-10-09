using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MisFinanzas.Infrastructure.Data;

namespace MisFinanzas.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=C:\\DataBases\\MisFinanzas.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }




}
