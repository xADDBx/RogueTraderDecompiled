using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

[Serializable]
public abstract class BlueprintPatchOperation
{
	[NonSerialized]
	public string TargetGuid;

	public string FieldName;

	protected Type concreteBlueprintType;

	protected FieldInfo field;

	protected Type fieldType;

	protected object fieldHolder;

	public virtual void Apply(SimpleBlueprint bp)
	{
		object obj = bp;
		concreteBlueprintType = bp.GetType();
		string[] array = FieldName.Split('.');
		foreach (string text in array)
		{
			field = concreteBlueprintType.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				PFLog.Mods.Log($"Field {text} not found in {concreteBlueprintType} while inspecting {bp.name}");
				BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic;
				while ((concreteBlueprintType = concreteBlueprintType.BaseType) != null)
				{
					field = concreteBlueprintType.GetField(text, bindingAttr);
					if (field != null)
					{
						break;
					}
				}
				if (field == null)
				{
					PFLog.Mods.Exception(new Exception($"Field {text} not found in {concreteBlueprintType}, while parsing {bp.GetType()} , field {FieldName}"));
					return;
				}
				PFLog.Mods.Log("Field found among private fields of parent class, it's ok.");
			}
			fieldHolder = obj;
			obj = field.GetValue(obj);
			if (obj == null)
			{
				PFLog.Mods.Error($"Field {field.Name} or \"{text} \" has null value, while inspecting {concreteBlueprintType}, while parsing {bp.GetType()} , field {FieldName}");
				try
				{
					Type type = field.FieldType;
					ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
					if (constructor == null)
					{
						PFLog.Mods.Error($"The type {type} of {text} has no parameterless constructor, cannot create new instance. Aborting blueprint patch...");
						return;
					}
					obj = constructor.Invoke(null);
					if (obj == null)
					{
						PFLog.Mods.Error("Failed to create new instance with parameterless ctor for null field value. Aborting patch...");
						return;
					}
					field.SetValue(fieldHolder, obj);
				}
				catch (Exception ex)
				{
					PFLog.Mods.Error("Unable to create new instance while trying to fix null field value in bp : " + ex.ToString());
					return;
				}
			}
			concreteBlueprintType = obj.GetType();
		}
		fieldType = field.FieldType;
	}

	protected bool CheckTypeIsList(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(List<>);
		}
		return false;
	}

	protected bool CheckTypeIsArrayOrListOfBlueprintReferences(Type type)
	{
		if (!type.IsArray && !CheckTypeIsList(type))
		{
			return false;
		}
		Type listElementType = GetListElementType(type);
		PFLog.Mods.Log($"Got IList element type {listElementType} for {concreteBlueprintType} : {field.Name}");
		if (listElementType == null)
		{
			throw new Exception($"Failed to get ElementType for {concreteBlueprintType} : {field.Name}");
		}
		return typeof(BlueprintReferenceBase).IsAssignableFrom(listElementType);
	}

	protected static Type GetListElementType(Type type)
	{
		Type type2 = type.GetInterfaces().FirstOrDefault((Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<object>).GetGenericTypeDefinition());
		if ((object)type2 == null)
		{
			return null;
		}
		return type2.GetGenericArguments()[0];
	}

	public override string ToString()
	{
		return "BlueprintPatchOperation for GUID: " + TargetGuid + " and field: " + FieldName + " ";
	}
}
