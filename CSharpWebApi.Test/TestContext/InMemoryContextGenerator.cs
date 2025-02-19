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
