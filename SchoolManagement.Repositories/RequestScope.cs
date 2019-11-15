using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Repositories
{
    public class RequestScope
    {
        public RequestScope(IServiceProvider serviceProvider, IMapper mapper, string userId, int? tenantId)
        {
            this.ServiceProvider = serviceProvider;
            this.UserId = userId;
            this.TenantId = tenantId;
            this.Mapper = mapper;
        }

        public IServiceProvider ServiceProvider { get; }
        public string UserId { get; }
        public int? TenantId { get; }
        public IMapper Mapper { get; }
    }

    public class RequestScope<Context> : RequestScope
        where Context : SMEfContext
    {
        public RequestScope(IServiceProvider serviceProvider, Context context, IMapper mapper, string userId, int? tenantId)
            : base(serviceProvider, mapper, userId, tenantId)
        {
            this.DbContext = context;
        }

        public Context DbContext { get; }
    }
}
