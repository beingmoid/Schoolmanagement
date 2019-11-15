using AutoMapper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SchoolManagement.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SchoolManagement.Data
{
    class SchoolContext : SMEfContext
    {
        public SchoolContext(IRequestInfo requestInfo, string connectionString) : base(requestInfo, connectionString)
        {
        }

        protected override void InitializeEntities()
        {
            throw new NotImplementedException();
        }

        protected override void SeedStaticData(ModelBuilder modelBuilder)
        {
            throw new NotImplementedException();
        }

        protected override void SeedTestingData(ModelBuilder modelBuilder)
        {
            throw new NotImplementedException();
        }
    }
    public class SchoolContextFactory : IDesignTimeDbContextFactory<SchoolContext>
    {
        SchoolContext IDesignTimeDbContextFactory<SchoolContext>.CreateDbContext(string[] args)
        {
            AutoMapper.Configuration.IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath($"{Directory.GetParent(Directory.GetCurrentDirectory())}/SchoolManagement.Api")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Staging.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return new SchoolContext(new RequestInfo(configuration, null));
        }
    }
}
