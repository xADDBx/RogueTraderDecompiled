using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.PubSubSystem.Core;

public static class InterfaceFinder<TSubscriber> where TSubscriber : class
{
	private static readonly Dictionary<Type, List<IGrouping<Type, Type>>> s_InterfaceCache;

	private static readonly Dictionary<Type, List<Type>> s_RulebookTypesCache;

	static InterfaceFinder()
	{
		s_InterfaceCache = new Dictionary<Type, List<IGrouping<Type, Type>>>();
		s_RulebookTypesCache = new Dictionary<Type, List<Type>>();
		GenericStaticTypesHolder.Add(typeof(InterfaceFinder<TSubscriber>));
	}

	public static List<Type> GetSubscribedRulebookTypes(TSubscriber subscriber)
	{
		Type type = subscriber.GetType();
		if (!s_RulebookTypesCache.TryGetValue(type, out var value))
		{
			value = (from it in subscriber.GetType().GetInterfaces()
				where it.Implements<TSubscriber>() && it != typeof(TSubscriber)
				select it into i
				where i.IsGenericType
				where i.GetGenericArguments().Length == 1
				where typeof(IRulebookEvent).IsAssignableFrom(i.GetGenericArguments()[0])
				where typeof(IRulebookHandler<>).MakeGenericType(i.GetGenericArguments()).IsAssignableFrom(i)
				select i.GetGenericArguments()[0]).ToList();
			s_RulebookTypesCache[type] = value;
		}
		return value;
	}

	public static IEnumerable<Type> GetSubscriberInterfaces<TTag>(TSubscriber subscriber)
	{
		Type type = subscriber.GetType();
		if (!s_InterfaceCache.TryGetValue(type, out var value))
		{
			IEnumerable<Type> ifaces = from i in type.GetInterfaces()
				where i.Implements<ISubscriber>() && i != typeof(ISubscriber)
				select i;
			value = ifaces.Where((Type i) => !i.IsGenericType).GroupBy(delegate(Type i)
			{
				Type type2 = ifaces.FirstOrDefault((Type it) => it.GetInterfaces().Contains(i));
				return ((object)type2 == null || !type2.IsGenericType) ? typeof(EventTagNone) : type2.GetGenericArguments()[0];
			}).ToList();
			s_InterfaceCache[type] = value;
		}
		IEnumerable<Type> enumerable = value.Find((IGrouping<Type, Type> g) => g.Key == typeof(TTag));
		return enumerable ?? Enumerable.Empty<Type>();
	}
}
