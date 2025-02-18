using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CSharpWebApi.Test.TestContext
{
    public static class InMemoryContextGenerator
    {
        public static T Generate<T>() where T : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

            return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options)!;
        }
    }
}
