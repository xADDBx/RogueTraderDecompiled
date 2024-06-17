using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.EntitySystem;

public class EntityFactService : IService, IDisposable
{
	private static IList[] s_FactsByType;

	private static Dictionary<Type, int> s_TypeIndex;

	private static ServiceProxy<EntityFactService> s_InstanceProxy;

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public static EntityFactService Instance
	{
		get
		{
			if (s_InstanceProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new EntityFactService());
				s_InstanceProxy = Services.GetProxy<EntityFactService>();
			}
			return s_InstanceProxy.Instance;
		}
	}

	public static int FactIndexCount => s_TypeIndex.Count;

	static EntityFactService()
	{
		s_TypeIndex = new Dictionary<Type, int>();
		Type[] array = (from t in typeof(EntityFact).GetSubclasses().Concat(typeof(EntityFact))
			orderby t.FullName
			select t).ToArray();
		s_FactsByType = new IList[array.Length];
		IEnumerable<Type> enumerable = array.Where((Type i) => i.ContainsGenericParameters);
		if (enumerable.Any())
		{
			string text = string.Join(", ", enumerable);
			throw new Exception("Non abstract EntityFacts with generic parameters are forbidden: " + text);
		}
		for (int j = 0; j < array.Length; j++)
		{
			Type type = array[j];
			Type type2 = typeof(List<>).MakeGenericType(type);
			s_FactsByType[j] = (IList)Activator.CreateInstance(type2);
			s_TypeIndex[type] = j;
			typeof(Indexer<>).MakeGenericType(type).GetField("Index", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, j);
		}
	}

	public void Dispose()
	{
		IList[] array = s_FactsByType;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clear();
		}
	}

	public static int GetFactIndex<T>()
	{
		return Indexer<T>.Index;
	}

	public static int GetFactIndex(Type type)
	{
		if (!s_TypeIndex.TryGetValue(type, out var value))
		{
			return -1;
		}
		return value;
	}

	public IReadOnlyList<T> Get<T>()
	{
		return (IReadOnlyList<T>)s_FactsByType[Indexer<T>.Index];
	}

	public void Register(EntityFact fact)
	{
		Type type = fact.GetType();
		int num = s_TypeIndex[type];
		s_FactsByType[num].Add(fact);
	}

	public void Unregister(EntityFact fact)
	{
		Type type = fact.GetType();
		int num = s_TypeIndex[type];
		s_FactsByType[num].Remove(fact);
	}
}
