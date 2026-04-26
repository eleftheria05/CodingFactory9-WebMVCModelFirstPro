using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolApp.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static SchoolMvc9proContext Create()
        {
            DbContextOptions<SchoolMvc9proContext> options;


            options = new DbContextOptionsBuilder<SchoolMvc9proContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new SchoolMvc9proContext(options);
        }
    }
}