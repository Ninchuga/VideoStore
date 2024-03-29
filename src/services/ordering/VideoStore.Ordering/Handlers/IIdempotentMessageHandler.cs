﻿using MassTransit;
using VideoStore.Ordering.Infrastrucutre;

namespace VideoStore.Ordering.Handlers
{
    public interface IIdempotentMessageHandler<T> where T : class
    {
        Task Handle(ConsumeContext<T> message, string consumerName, CancellationTokenSource cancellationTokenSource, Func<OrderingContext, Task> function);
    }
}