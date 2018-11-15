using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi
{
    internal static class Initializer
    {
        private static bool _initialized = false;
        private static object _lock = new object();

        public static void Seed(TodoContext context)
        {

        }

        internal static void Initialize(TodoContext context)
        {
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (_initialized)
                    {
                        return;
                    }
                    InitializeData(context);
                }
            }
        }

        private static void InitializeData(TodoContext context)
        {
            context.Database.Migrate();
            Seed(context);
        }
    }
}