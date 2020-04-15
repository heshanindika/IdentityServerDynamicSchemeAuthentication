using IdentityServerAspNetIdentity.Data.Migrations.TenantManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Data.Repository
{
    public class TenantInfoRepository : ITenantInfoRepository
    {
        private readonly TenantManagementDbContext tenantManagementDbContext;

        public TenantInfoRepository(TenantManagementDbContext tenantManagementDbContext)
        {
            this.tenantManagementDbContext = tenantManagementDbContext;
        }

        public TenantInfo Get(string domainName)
        {
            var tenant = tenantManagementDbContext.TenantInfos.Include("TenantAuthenticatingSchemes.ExternalAuthenticatingScheme").FirstOrDefault(x => x.DomainName == domainName.ToLower());
            if(tenant != null)
            {
                return new TenantInfo { TenantId = tenant.TenantId, Schemes = tenant.TenantAuthenticatingSchemes.Select(x => x.ExternalAuthenticatingScheme.SchemeName).ToList() };
            }

            return null;
        }
    }
}
