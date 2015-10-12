using Database.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DataContext : DbContext
    {
        public DataContext(string connectionString) : base(connectionString)
        {
        }

        public DataContext() 
        {
        }

        public DbSet<Deltaker> Deltakere { get;set; }

        public DbSet<Lag> Lag { get; set; }

        public DbSet<Melding> Meldinger { get; set; }

        public DbSet<DeltakerPosisjon> DeltakerPosisjoner{ get; set; }

        public DbSet<LagIMatch> LagIMatch { get; set; }

        public DbSet<Match> Matcher { get; set; }

        public DbSet<Post> Poster { get; set; }

        public DbSet<PostIMatch> PosterIMatch { get; set; }

        public DbSet<PostRegistrering> PostRegisteringer { get; set; }

        public DbSet<Vaapen> Våpen { get; set; }

        public DbSet<VaapenBeholdning> VåpenBeholdning { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();    
        }
    }
}
