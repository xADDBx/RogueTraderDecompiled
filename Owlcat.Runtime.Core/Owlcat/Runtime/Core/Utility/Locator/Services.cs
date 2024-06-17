using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Owlcat.Runtime.Core.Utility.Locator;

public static class Services
{
	private static readonly Dictionary<Type, ServiceProxy> s_RegisteredServices = new Dictionary<Type, ServiceProxy>();

	public static IEnumerable<IService> AllServices => from i in s_RegisteredServices.Values
		select i.Service into i
		where i != null
		select i;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeServices()
	{
		RegisterDefaultServices();
	}

	public static void RegisterServiceInstance<T>([NotNull] T service) where T : class, IService
	{
		Type typeFromHandle = typeof(T);
		if (s_RegisteredServices.TryGetValue(typeFromHandle, out var value))
		{
			value.Dispose();
		}
		s_RegisteredServices[typeFromHandle] = new ServiceProxy<T>
		{
			Service = service
		};
	}

	public static void DisposeServiceInstance<T>() where T : class, IService
	{
		Type typeFromHandle = typeof(T);
		if (s_RegisteredServices.TryGetValue(typeFromHandle, out var value))
		{
			value.Dispose();
			s_RegisteredServices.Remove(typeFromHandle);
		}
	}

	public static void EndLifetime(ServiceLifetimeType lifetime)
	{
		foreach (KeyValuePair<Type, ServiceProxy> item in s_RegisteredServices.ToList())
		{
			if (item.Value.Service.Lifetime == lifetime)
			{
				item.Value.Dispose();
				s_RegisteredServices.Remove(item.Key);
			}
		}
	}

	public static T GetInstance<T>() where T : class, IService
	{
		s_RegisteredServices.TryGetValue(typeof(T), out var value);
		return value?.Service as T;
	}

	public static ServiceProxy<T> GetProxy<T>() where T : class, IService
	{
		s_RegisteredServices.TryGetValue(typeof(T), out var value);
		return value as ServiceProxy<T>;
	}

	public static void ResetAllRegistrations()
	{
		try
		{
			foreach (KeyValuePair<Type, ServiceProxy> s_RegisteredService in s_RegisteredServices)
			{
				s_RegisteredService.Value.Dispose();
			}
		}
		finally
		{
			s_RegisteredServices.Clear();
		}
	}

	public static void RegisterDefaultServices()
	{
		if (GetInstance<Repository>() == null)
		{
			RegisterServiceInstance(new Repository());
		}
	}
}
