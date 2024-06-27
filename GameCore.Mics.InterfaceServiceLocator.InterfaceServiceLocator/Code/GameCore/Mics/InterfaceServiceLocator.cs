using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.GameCore.Mics;

public static class InterfaceServiceLocator
{
	private static readonly Dictionary<Type, InterfaceService> s_RegisteredServices = new Dictionary<Type, InterfaceService>();

	public static void RegisterService<T>([NotNull] T service, Type targetInterface) where T : class, InterfaceService
	{
		Exception ex = CheckTypeToRegisterInternal(targetInterface);
		if (ex != null)
		{
			throw ex;
		}
		if (!targetInterface.IsInterface)
		{
			throw new Exception($"Given desired type {targetInterface} is not an interface. " + "Only interface types should be given (not classes or structs)! ");
		}
		if (!typeof(InterfaceService).IsAssignableFrom(targetInterface))
		{
			throw new Exception($"Given interface {targetInterface} is not derived from {typeof(InterfaceService)}");
		}
		if (s_RegisteredServices.TryGetValue(targetInterface, out var _))
		{
			throw new Exception($"Given target interface {targetInterface} already registered!");
		}
		s_RegisteredServices[targetInterface] = service;
	}

	public static void UnregisterService([NotNull] Type targetInterface)
	{
		if (!targetInterface.IsInterface)
		{
			throw new Exception($"Target type {targetInterface} is not an interface.");
		}
		if (!typeof(InterfaceService).IsAssignableFrom(targetInterface))
		{
			throw new Exception($"Target type {targetInterface} is not inherited from InterfaceService");
		}
		if (s_RegisteredServices.ContainsKey(targetInterface))
		{
			s_RegisteredServices.Remove(targetInterface);
		}
	}

	public static T GetService<T>() where T : class, InterfaceService
	{
		Type typeFromHandle = typeof(T);
		Exception ex = CheckTypeToGetInternal(typeFromHandle);
		if (ex != null)
		{
			throw ex;
		}
		if (s_RegisteredServices.TryGetValue(typeFromHandle, out var value))
		{
			return (T)value;
		}
		throw new Exception($"Service for given interface {typeFromHandle} not registered!");
	}

	public static T TryGetService<T>() where T : class, InterfaceService
	{
		Type typeFromHandle = typeof(T);
		Exception ex = CheckTypeToGetInternal(typeFromHandle);
		if (ex != null)
		{
			Debug.LogError(ex);
			return null;
		}
		if (s_RegisteredServices.TryGetValue(typeFromHandle, out var value))
		{
			return (T)value;
		}
		return null;
	}

	private static Exception CheckTypeToRegisterInternal(Type targetInterface)
	{
		Exception ex = CheckTypeToGetInternal(targetInterface);
		if (ex != null)
		{
			return ex;
		}
		if (!s_RegisteredServices.TryGetValue(targetInterface, out var _))
		{
			return null;
		}
		return new Exception($"Given target interface {targetInterface} already registered!");
	}

	private static Exception CheckTypeToGetInternal(Type targetInterface)
	{
		if (!targetInterface.IsInterface)
		{
			return new Exception($"Given desired type {targetInterface} is not an interface. " + "Only interface types should be given (not classes or structs)! ");
		}
		if (!typeof(InterfaceService).IsAssignableFrom(targetInterface))
		{
			return new Exception($"Given interface {targetInterface} is not derived from {typeof(InterfaceService)}");
		}
		return null;
	}

	private static void LogServicesStorage()
	{
		Debug.LogError($"Number of registered services: {s_RegisteredServices.Count}");
		foreach (KeyValuePair<Type, InterfaceService> s_RegisteredService in s_RegisteredServices)
		{
			Debug.LogError($"{s_RegisteredService.Key} : {s_RegisteredService.Value}");
		}
	}
}
