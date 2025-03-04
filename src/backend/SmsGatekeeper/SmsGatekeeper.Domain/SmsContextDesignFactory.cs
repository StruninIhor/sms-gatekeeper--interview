using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsGatekeeper.Domain
{
    public class SmsContextDesignFactory : IDesignTimeDbContextFactory<SmsContext>
    {
        public SmsContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
            .Build();

            var builder = new DbContextOptionsBuilder<SmsContext>();
            var connectionString = configuration.GetConnectionString("SmsDatabase");

            builder.UseSqlServer(connectionString);

            return new SmsContext(builder.Options);
        }
    }
}
