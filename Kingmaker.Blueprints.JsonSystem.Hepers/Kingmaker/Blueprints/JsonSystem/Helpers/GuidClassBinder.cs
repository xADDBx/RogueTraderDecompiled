using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Reflection;
using Newtonsoft.Json.Serialization;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

public class GuidClassBinder : ISerializationBinder
{
	private static readonly Lazy<Task<(IDictionary<string, Type> GuidToTypeCache, IDictionary<Type, string> TypeToGuidCache)>> WarmUpTask = new Lazy<Task<(IDictionary<string, Type>, IDictionary<Type, string>)>>(() => Task.Run((Func<(IDictionary<string, Type>, IDictionary<Type, string>)>)PreWarmCache));

	private static IDictionary<string, Type> GuidToTypeCache => WarmUpTask.Value.Result.GuidToTypeCache;

	private static IDictionary<Type, string> TypeToGuidCache => WarmUpTask.Value.Result.TypeToGuidCache;

	public static void StartWarmingUp()
	{
		_ = WarmUpTask.Value;
	}

	public Type BindToType(string assemblyName, string typeName)
	{
		if (GuidToTypeCache.TryGetValue(typeName, out var value))
		{
			return value;
		}
		throw new Exception("Cannot find type for guid: " + typeName + ", " + assemblyName);
	}

	public void BindToName(Type serializedType, out string assemblyName, out string typeName)
	{
		assemblyName = serializedType.Name;
		if (TypeToGuidCache.TryGetValue(serializedType, out typeName))
		{
			return;
		}
		throw new ArgumentException("Type " + serializedType.FullName + " cannot be resolved as GUID", "serializedType");
	}

	private static string GetGuidOfType(Type type)
	{
		if (!type.CustomAttributes.Any((CustomAttributeData v) => v.AttributeType == typeof(TypeIdAttribute)))
		{
			return "";
		}
		return type.GetCustomAttribute<TypeIdAttribute>().GuidString;
	}

	private static (IDictionary<string, Type> GuidToTypeCache, IDictionary<Type, string> TypeToGuidCache) PreWarmCache()
	{
		Dictionary<Type, string> dictionary = new Dictionary<Type, string>();
		Dictionary<string, Type> dictionary2 = new Dictionary<string, Type>();
		foreach (List<(Type, string)> typeFieldInNonStandardAssembly in AppDomain.CurrentDomain.GetTypeFieldInNonStandardAssemblies<List<(Type, string)>>("ClassesWithGuid", "Classes"))
		{
			foreach (var item2 in typeFieldInNonStandardAssembly)
			{
				dictionary.Add(item2.Item1, item2.Item2);
				dictionary2.Add(item2.Item2, item2.Item1);
			}
		}
		if (dictionary2.Count == 0)
		{
			PFLog.Default.Error("ERROR! Could not find guids for types in generated code. Falling back to reflection. Please report to Maxim Savenkov!");
			ParallelQuery<(Type Type, string Guid)> source = from v in AppDomain.CurrentDomain.GetAssembliesSafe().AsParallel().SelectMany((Assembly v) => v.GetTypesSafe())
				where !v.IsAbstract
				select (Type: v, Guid: GetGuidOfType(v)) into v
				where !string.IsNullOrEmpty(v.Guid)
				select v;
			return new ValueTuple<IDictionary<string, Type>, IDictionary<Type, string>>(item2: source.ToDictionary(((Type Type, string Guid) v) => v.Type, ((Type Type, string Guid) v) => v.Guid), item1: source.ToDictionary(((Type Type, string Guid) v) => v.Guid, ((Type Type, string Guid) v) => v.Type));
		}
		return (GuidToTypeCache: dictionary2, TypeToGuidCache: dictionary);
	}
}
