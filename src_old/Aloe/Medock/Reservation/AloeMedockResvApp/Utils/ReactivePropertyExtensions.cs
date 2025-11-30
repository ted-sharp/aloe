using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Utils;

public static class ReactivePropertyExtensions
{
    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<T, Task> funcAsync,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(funcAsync, nameof(funcAsync));

        return source.Subscribe(async void (x) =>
        {
            try
            {
                await funcAsync(x);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error!");
            }
        });
    }
}
