using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility.Core;

public class OptInContractResolver : DefaultContractResolver
{
	private static readonly Type[] s_EntityConstructorTypes = new Type[1] { typeof(JsonConstructorMark) };

	private static readonly object[] s_EntityConstructorParams = new object[1] { default(JsonConstructorMark) };

	private bool IsOurType(string assemblyName)
	{
		if (!(assemblyName == "Code") && !assemblyName.Contains("Kingmaker"))
		{
			return assemblyName.Contains("Owlcat");
		}
		return true;
	}

	protected override JsonObjectContract CreateObjectContract(Type objectType)
	{
		JsonObjectContract jsonObjectContract = base.CreateObjectContract(objectType);
		if (objectType.IsValueType)
		{
			jsonObjectContract.IsReference = false;
		}
		string name = objectType.Assembly.GetName().Name;
		if (objectType.IsInterface || objectType.IsValueType || !IsOurType(name))
		{
			return jsonObjectContract;
		}
		ConstructorInfo[] constructors = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		ConstructorInfo ctor = objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, s_EntityConstructorTypes, null);
		if (ctor != null)
		{
			jsonObjectContract.OverrideCreator = (object?[] p) => ctor.Invoke(s_EntityConstructorParams);
			jsonObjectContract.CreatorParameters.Clear();
		}
		else if (!constructors.Any((ConstructorInfo c) => c.HasAttribute<JsonConstructorAttribute>()))
		{
			if (typeof(IEntity).IsAssignableFrom(objectType))
			{
				PFLog.Default.Error("Can't find json constructor for " + objectType.FullName);
				return jsonObjectContract;
			}
			ctor = constructors.FirstOrDefault((ConstructorInfo c) => c.GetParameters().Empty());
			if (ctor == null)
			{
				PFLog.Default.Error("Can't find json constructor for " + objectType.FullName);
				return jsonObjectContract;
			}
			jsonObjectContract.OverrideCreator = (object?[] p) => ctor.Invoke(null);
			jsonObjectContract.CreatorParameters.Clear();
		}
		return jsonObjectContract;
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		if (type.Assembly == typeof(Vector3).Assembly)
		{
			return base.CreateProperties(type, memberSerialization);
		}
		return base.CreateProperties(type, MemberSerialization.OptIn);
	}

	protected override List<MemberInfo> GetSerializableMembers(Type objectType)
	{
		if (BuiltInUnityTypesSerializableMembers.TryGetMembers(objectType, out var result))
		{
			return result;
		}
		return base.GetSerializableMembers(objectType);
	}
}
