using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

public class BlueprintFieldsTraverser
{
	private static readonly Dictionary<Type, List<FieldInfo>> FieldsCache = new Dictionary<Type, List<FieldInfo>>();

	public static IEnumerable<MemberInfo> GetUnitySerializedFields(Type type)
	{
		if (!FieldsCache.TryGetValue(type, out var value))
		{
			value = (from f in FieldsContractResolver.GetUnitySerializedFields(type)
				where !f.HasAttribute<JsonIgnoreAttribute>()
				where !f.HasAttribute<ExcludeFieldFromBuildAttribute>()
				select f).Cast<FieldInfo>().ToList();
			FieldsCache[type] = value;
		}
		return value;
	}
}
