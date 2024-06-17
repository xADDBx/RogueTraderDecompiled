using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Owlcat.Runtime.Core.Registry;

internal class Repository : IService
{
	private readonly Dictionary<Type, IObjectRegistryBase> m_RegistryCache = new Dictionary<Type, IObjectRegistryBase>();

	private static ServiceProxy<Repository> s_Proxy;

	public static Repository Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<Repository>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.EditorSession;

	internal IEnumerable<IObjectRegistryBase> AllRegistries => m_RegistryCache.Values;

	public IObjectRegistryBase GetRegistry(Type t)
	{
		if (m_RegistryCache.TryGetValue(t, out var value))
		{
			return value;
		}
		value = new ObjectRegistryNonGeneric();
		m_RegistryCache[t] = value;
		return value;
	}

	public ObjectRegistry<T> GetRegistry<T>() where T : class
	{
		Type typeFromHandle = typeof(T);
		ObjectRegistry<T> objectRegistry;
		if (!m_RegistryCache.TryGetValue(typeFromHandle, out var value))
		{
			objectRegistry = new ObjectRegistry<T>();
		}
		else if (value is ObjectRegistry<T>)
		{
			objectRegistry = (ObjectRegistry<T>)value;
		}
		else
		{
			if (!(value is ObjectRegistryNonGeneric or))
			{
				throw new Exception("Registry for " + typeFromHandle.Name + " is of unexpected type " + value.GetType().Name);
			}
			objectRegistry = ObjectRegistry<T>.CreateFromNonGeneric(or);
		}
		m_RegistryCache[typeFromHandle] = objectRegistry;
		return objectRegistry;
	}
}
