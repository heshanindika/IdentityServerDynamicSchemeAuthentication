using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerAspNetIdentity.Data.Repository
{
    public class TenantInfo
    {
        public int TenantId { get; set; }
        public List<string> Schemes { get; set; }
    }
}
