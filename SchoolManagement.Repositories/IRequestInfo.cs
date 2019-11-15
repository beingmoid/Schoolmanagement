using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolManagement.Repositories
{
   public interface IRequestInfo
    {
        IConfiguration Configuration { get; }
    }
}
