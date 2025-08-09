using System.Collections.Concurrent;

namespace Bobs_Corn_API.Services
{
    public class TotalsStore
    {
        private readonly ConcurrentDictionary<string, int> _totals = new();

        public int Increment(string clientKey)
            => _totals.AddOrUpdate(clientKey, 1, (_, prev) => prev + 1);
    }
}
