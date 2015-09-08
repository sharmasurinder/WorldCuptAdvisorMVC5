using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
//using System.Data.Entity.ModelConfiguration.Conventions;

namespace WorldCupAdvisorMVC.Model
{
    public class WorldCupAdvisorContext : DbContext
    {
        public WorldCupAdvisorContext(): base("DefaultConnection")
        {

        }

        public DbSet<UserProfile> UserProfile { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
        }
    }
}
