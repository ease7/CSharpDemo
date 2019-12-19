using Microsoft.EntityFrameworkCore;
using System;

namespace IdiomsSolitaire.Core
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<ChengYu> ChengYu { get; set; }
    }
}
