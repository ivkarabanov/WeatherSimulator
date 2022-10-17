using System;
using System.Collections.Generic;
using System.Linq;
using WeatherSimulator.Server.Storages.Abstractions;
using WeatherSimulator.Server.Models;
using System.Collections.Concurrent;
using System.Threading;

namespace WeatherSimulator.Server.Storages;

public class MeasureSubscriptionStore : IMeasureSubscriptionStore
{
	private readonly object _locker = new();
	private readonly ConcurrentDictionary<Guid, HashSet<SensorMeasureSubscription>> _subscriptionsDict = new();

	public IReadOnlyCollection<SensorMeasureSubscription> GetSubscriptions(Guid sensorId)
	{
		if (!_subscriptionsDict.TryGetValue(sensorId, out var sensorSubscription))
		{
			return Array.Empty<SensorMeasureSubscription>();
		}

		SensorMeasureSubscription[] items;
		lock (_locker)
		{
			items = sensorSubscription.ToArray();
		}

		return items;
	}

	public void RemoveSubscription(Guid sensorId, Guid subscriptionId)
	{
		if (!_subscriptionsDict.TryGetValue(sensorId, out var sensorSubscription))
		{
			return;
		}

		//HACK: Подписки равные, если у них одинаковый id и sensorId. Создаем фейковую подписку, чтобы удалить настоящую из set-а.
		SensorMeasureSubscription sub = new(subscriptionId, sensorId, CancellationToken.None, null!);

		lock (_locker)
		{
			sensorSubscription.Remove(sub);
		}
	}

	public void AddSubscription(SensorMeasureSubscription subscription)
	{
		if (_subscriptionsDict.TryGetValue(subscription.SensorId, out var sensorSubscription))
		{
			lock (_locker)
			{
				sensorSubscription.Add(subscription);
				return;
			}
		}

		sensorSubscription = new HashSet<SensorMeasureSubscription>
		{
			subscription
		};

		_subscriptionsDict.AddOrUpdate(subscription.SensorId,
			sensorSubscription,
			(_, set) =>
			{
				set.Add(subscription);
				return set;
			});
	}
}
