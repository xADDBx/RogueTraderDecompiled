using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Persistence.JsonUtility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kingmaker.Networking.Serialization;

public class GameStateContractResolver : OptInContractResolver
{
	private static readonly Type AddMemberAttribute = typeof(GameStateIncludeAttribute);

	private static readonly Type DelMemberAttribute = typeof(GameStateIgnoreAttribute);

	protected override List<MemberInfo> GetSerializableMembers(Type objectType)
	{
		List<MemberInfo> serializableMembers = base.GetSerializableMembers(objectType);
		AddFieldsWithAttributes(objectType, AddMemberAttribute, serializableMembers);
		AddPropertiesWithAttributes(objectType, AddMemberAttribute, serializableMembers);
		serializableMembers.RemoveAll((MemberInfo memberInfo) => Attribute.IsDefined(memberInfo, DelMemberAttribute, inherit: false));
		return serializableMembers;
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);
		foreach (JsonProperty item in list)
		{
			if (!item.HasMemberAttribute || item.Ignored || !item.Readable)
			{
				IList<Attribute> list2 = item.AttributeProvider?.GetAttributes(AddMemberAttribute, inherit: false);
				if (list2 != null && 0 < list2.Count)
				{
					item.HasMemberAttribute = true;
					item.Ignored = false;
					item.Readable = true;
				}
			}
		}
		return list;
	}

	private static void AddFieldsWithAttributes(Type objectType, Type attributeType, List<MemberInfo> list)
	{
		List<MemberInfo> list2 = new List<MemberInfo>(objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		GetChildPrivateFields(list2, objectType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MemberInfo item in list2)
		{
			if (Attribute.IsDefined(item, attributeType, inherit: false) && !list.Contains(item))
			{
				list.Add(item);
			}
		}
		static void GetChildPrivateFields(List<MemberInfo> fields, Type targetType, BindingFlags bindingAttr)
		{
			if ((bindingAttr & BindingFlags.NonPublic) != 0)
			{
				BindingFlags bindingAttr2 = RemoveFlag(bindingAttr, BindingFlags.Public);
				while ((object)(targetType = targetType.BaseType) != null)
				{
					IEnumerable<FieldInfo> collection = from f in targetType.GetFields(bindingAttr2)
						where f.IsPrivate
						select f;
					fields.AddRange(collection);
				}
			}
		}
	}

	private static void AddPropertiesWithAttributes(Type objectType, Type attributeType, List<MemberInfo> list)
	{
		List<PropertyInfo> list2 = new List<PropertyInfo>(objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		GetChildPrivateProperties(list2, objectType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo item in list2)
		{
			if (Attribute.IsDefined(item, attributeType, inherit: false) && !list.Contains(item))
			{
				list.Add(item);
			}
		}
		static MethodInfo GetBaseDefinition(PropertyInfo propertyInfo)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
			if ((object)getMethod == null)
			{
				return propertyInfo.GetSetMethod(nonPublic: true)?.GetBaseDefinition();
			}
			return getMethod.GetBaseDefinition();
		}
		static void GetChildPrivateProperties(List<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
		{
			while ((object)(targetType = targetType.BaseType) != null)
			{
				PropertyInfo[] properties = targetType.GetProperties(bindingAttr);
				foreach (PropertyInfo propertyInfo2 in properties)
				{
					PropertyInfo subTypeProperty = propertyInfo2;
					if (!IsVirtual(subTypeProperty))
					{
						if (!IsPublic(subTypeProperty))
						{
							int num = initialProperties.FindIndex((PropertyInfo p) => p.Name == subTypeProperty.Name);
							if (num == -1)
							{
								initialProperties.Add(subTypeProperty);
							}
							else if (!IsPublic(initialProperties[num]))
							{
								initialProperties[num] = subTypeProperty;
							}
						}
						else if (initialProperties.FindIndex((PropertyInfo p) => string.Equals(p.Name, subTypeProperty.Name, StringComparison.Ordinal) && p.DeclaringType == subTypeProperty.DeclaringType) == -1)
						{
							initialProperties.Add(subTypeProperty);
						}
					}
					else
					{
						Type type = GetBaseDefinition(subTypeProperty)?.DeclaringType;
						if ((object)type == null)
						{
							type = subTypeProperty.DeclaringType;
						}
						Type subTypePropertyDeclaringType = type;
						if (initialProperties.FindIndex((PropertyInfo p) => p.Name.Equals(subTypeProperty.Name, StringComparison.Ordinal) && IsVirtual(p) && (GetBaseDefinition(p)?.DeclaringType ?? p.DeclaringType).IsAssignableFrom(subTypePropertyDeclaringType)) == -1)
						{
							initialProperties.Add(subTypeProperty);
						}
					}
				}
			}
		}
		static bool IsPublic(PropertyInfo property)
		{
			MethodInfo getMethod2 = property.GetGetMethod();
			if ((object)getMethod2 != null && getMethod2.IsPublic)
			{
				return true;
			}
			return property.GetSetMethod()?.IsPublic ?? false;
		}
		static bool IsVirtual(PropertyInfo propertyInfo)
		{
			MethodInfo getMethod3 = propertyInfo.GetGetMethod(nonPublic: true);
			if ((object)getMethod3 != null && getMethod3.IsVirtual)
			{
				return true;
			}
			return propertyInfo.GetSetMethod(nonPublic: true)?.IsVirtual ?? false;
		}
	}

	private static BindingFlags RemoveFlag(BindingFlags bindingAttr, BindingFlags flag)
	{
		if ((bindingAttr & flag) == flag)
		{
			return bindingAttr ^ flag;
		}
		return bindingAttr;
	}
}
