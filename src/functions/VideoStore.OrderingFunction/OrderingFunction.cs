using Microsoft.Extensions.Logging;
using VideoStore.OrderingFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using System.Text.Json;

namespace VideoStore.OrderingFunction
{
    public class OrderingFunction
    {
        private readonly ILogger<OrderingFunction> _logger;

        public OrderingFunction(ILogger<OrderingFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderingFunction))]
        public void Run(
            [SqlTrigger(tableName: "[OrderingDb].[dbo].[Orders]", connectionStringSetting: "SqlConnectionString")]
            IReadOnlyList<SqlChange<Order>> changes)
        {
            if (changes is null)
                return;

            try
            {
                foreach (var change in changes)
                {
                    var movies = change.Item?.Movies is not null ? JsonSerializer.Deserialize<List<Movie>>(change.Item.Movies) : new();
                    _logger.LogInformation("Order operation: '{Operation}' for order id: {OrderId}, and user: '{UserEmail}' with price: {Price}, and movies {Movies}.",
                        change.Operation, change.Item?.Id, change.Item?.UserEmail, change.Item?.Price, string.Join(", ", movies)); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ocurred while executing the {FunctionName} with error: {Error}", nameof(OrderingFunction), ex.Message);
            }
        }
    }
}
