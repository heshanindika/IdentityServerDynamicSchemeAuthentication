using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Data.Migrations.TenantManagement
{
    public class TenantManagementDbContext: DbContext
    {
        public TenantManagementDbContext(DbContextOptions<TenantManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<TenantInfo> TenantInfos { get; set; }
        public DbSet<TenantInfo> ExternalAuthenticatingScheme { get; set; }
        public DbSet<TenantInfo> TenantAuthenticatingScheme { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantAuthenticatingScheme>().HasKey(sc => new { sc.TenantId, sc.SchemeId });

            modelBuilder.Entity<TenantAuthenticatingScheme>()
                        .HasOne<TenantInfo>(sc => sc.TenantInfo)
                        .WithMany(s => s.TenantAuthenticatingSchemes)
                        .HasForeignKey(sc => sc.TenantId);

            modelBuilder.Entity<TenantAuthenticatingScheme>()
                .HasOne<ExternalAuthenticatingScheme>(sc => sc.ExternalAuthenticatingScheme)
                .WithMany(s => s.TenantAuthenticatingSchemes)
                .HasForeignKey(sc => sc.SchemeId);
        }
    }

    public class TenantInfo
    {
        [Key]
        public int TenantId { get; set; }
        [Required]
        public string DomainName { get; set; }
        public IList<TenantAuthenticatingScheme> TenantAuthenticatingSchemes { get; set; }
    }

    public class ExternalAuthenticatingScheme
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SchemeName { get; set; }
        public IList<TenantAuthenticatingScheme> TenantAuthenticatingSchemes { get; set; }
    }

    public class TenantAuthenticatingScheme
    {
        public int TenantId { get; set; }
        public TenantInfo TenantInfo { get; set; }
        public int SchemeId { get; set; }
        public ExternalAuthenticatingScheme ExternalAuthenticatingScheme { get; set; }
    }
}
