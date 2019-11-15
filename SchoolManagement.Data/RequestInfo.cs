using AutoMapper.Configuration;
using SchoolManagement.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Data
{
    public class RequestInfo : IRequestInfo
    {
        public RequestInfo(IConfiguration configuration, int? tenantId)
        {
            this.Configuration = configuration;
            this.TenantId = tenantId;
        }

        public IConfiguration Configuration { get; }

        public int? TenantId { get; }
    }
}
