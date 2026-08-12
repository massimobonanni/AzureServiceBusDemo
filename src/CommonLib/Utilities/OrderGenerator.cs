using Bogus;
using CommonLib.Entities;

namespace CommonLib.Utilities
{
    /// <summary>
    /// Generates fake Order instances using Bogus (Faker.NET).
    /// </summary>
    public static class OrderGenerator
    {
        private static int _nextId = 1;

        /// <summary>
        /// Generates a collection of fake orders.
        /// </summary>
        /// <param name="count">Number of orders to generate.</param>
        /// <param name="seed">Optional deterministic seed (use to get repeatable data).</param>
        /// <param name="startId">Starting Id value (defaults to 1).</param>
        /// <returns>List of generated orders.</returns>
        public static IEnumerable<Order> Generate(int count, int? seed = null, int startId = 0)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (startId < 0) throw new ArgumentOutOfRangeException(nameof(startId));

            if (startId == 0)
            {
                startId = _nextId;
            }

            var faker = new Faker<Order>("en")
                .StrictMode(true)
                .RuleFor(o => o.Id, f => startId + f.IndexFaker)
                .RuleFor(o => o.CustomerName, f => f.Name.FullName())
                .RuleFor(o => o.OrderDate, f => f.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow))
                .RuleFor(o => o.TotalAmount, f => decimal.Round(f.Random.Decimal(5, 500), 2));

            if (seed.HasValue)
            {
                Randomizer.Seed = new Random(seed.Value);
            }

            var orders = faker.Generate(count);
            _nextId=orders.Max(o => o.Id) + 1; // Update the next Id for future calls
            
            return orders;
        }
    }
}