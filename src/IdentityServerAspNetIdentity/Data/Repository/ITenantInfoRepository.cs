using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Data.Repository
{
    public interface ITenantInfoRepository
    {
        TenantInfo Get(string domainName);
    }
}
