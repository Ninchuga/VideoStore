using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Sql;
using VideoStore.OrderingFunction.Models;

namespace VideoStore.OrderingFunction
{
    public static class OrderingFunction
    {
        [FunctionName(nameof(OrderingFunction))]
        public static void Run(
            [SqlTrigger(tableName: "[OrderingDb].[dbo].[Orders]", connectionStringSetting: "SqlConnectionString")]
            IReadOnlyList<SqlChange<Order>> changes,
            ILogger logger)
        {

            if (changes is null)
                return;

            foreach (var change in changes)
            {
                logger.LogInformation("Order operation: '{Operation}' for order id: {OrderId}, and user: '{UserEmail}' with price: {Price}.", 
                    change.Operation, change.Item.Id, change.Item.UserEmail, change.Item.Price);
            }
        }
    }
}
