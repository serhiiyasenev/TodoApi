using TodoApi.Models;

namespace TodoApi
{
    internal static class Initializer
    {
        private static bool _initialized;
        private static readonly Lock Lock = new();

        public static void Seed(TodoContext context)
        {

        }

        internal static void Initialize(TodoContext context)
        {
            if (_initialized) return;

            lock (Lock)
            {
                InitializeData(context);
                _initialized = true;
            }
        }

        private static void InitializeData(TodoContext context)
        {
            context.Database.EnsureCreated();
            Seed(context);
        }
    }
}