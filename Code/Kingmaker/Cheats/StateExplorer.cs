using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Core.Cheats;
using Core.Reflection;
using Kingmaker.AreaLogic.QuestSystem;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class StateExplorer
{
	private static class RootFinders
	{
		public delegate(object instance, Type type, int used) RootFinder(string[] parts);

		public static IReadOnlyDictionary<string, RootFinder> Finders;

		static RootFinders()
		{
			Finders = (from method in typeof(RootFinders).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				let attr = method.GetCustomAttribute<RootFinderAttribute>()
				where attr != null
				let del = (RootFinder)method.CreateDelegate(typeof(RootFinder))
				select (Prefix: attr.Prefix, del: del)).ToDictionary(((string Prefix, RootFinder del) v) => v.Prefix, ((string Prefix, RootFinder del) v) => v.del);
		}

		[RootFinder("$")]
		public static (object instance, Type type, int used) GameObj(string[] parts)
		{
			GameObject gameObject = GameObject.Find(parts[0]);
			if (gameObject == null)
			{
				throw new Exception("Object with name " + parts[0] + " not found");
			}
			if (parts.Length == 1)
			{
				return (instance: gameObject, type: gameObject.GetType(), used: 1);
			}
			Component component = gameObject.GetComponent(parts[1]);
			if (component != null)
			{
				return (instance: component, type: component.GetType(), used: 2);
			}
			return (instance: gameObject, type: gameObject.GetType(), used: 1);
		}

		[RootFinder("Quests")]
		public static (object instance, Type type, int used) Quests(string[] parts)
		{
			QuestBook questBook = Game.Instance.Player.QuestBook;
			return (instance: questBook, type: questBook.GetType(), used: 0);
		}

		[RootFinder("Game")]
		public static (object instance, Type type, int used) GameInstance(string[] parts)
		{
			Game instance = Game.Instance;
			return (instance: instance, type: instance.GetType(), used: 0);
		}
	}

	private static class Aliases
	{
		public delegate(object instance, Type type, int newIdx) Alias(object instance, Type type, int idx, string currentPart, string[] parts);

		public static IList<(string prefix, Alias alias)> KnownAliases;

		static Aliases()
		{
			KnownAliases = (from method in typeof(Aliases).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				let attr = method.GetCustomAttribute<AliasAttribute>()
				where attr != null
				let del = (Alias)method.CreateDelegate(typeof(Alias))
				select (Prefix: attr.Prefix, del: del)).ToArray();
		}
	}

	private static class CustomIndexers
	{
		public delegate(object instance, Type type) Indexer(object instance, Type type, string indexString);

		public static IList<(string prefix, Indexer alias)> Known;

		static CustomIndexers()
		{
			Known = (from method in typeof(CustomIndexers).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				let attr = method.GetCustomAttribute<CustomIndexerAttribute>()
				where attr != null
				let del = (Indexer)method.CreateDelegate(typeof(Indexer))
				select (Prefix: attr.Prefix, del: del)).ToArray();
		}

		[CustomIndexer("?")]
		public static (object instance, Type type) FindByToString(object obj, Type type, string indexString)
		{
			if (obj is IEnumerable source)
			{
				object obj2 = source.Cast<object>().First((object v) => v.ToString().Contains(indexString));
				return (instance: obj2, type: obj2.GetType());
			}
			throw new Exception($"Object {obj} is not IEnumerable, cannot iterate");
		}
	}

	private static readonly Regex _indexer = new Regex("(\\w+)\\[(.+)\\]");

	[Cheat(Name = "game_data")]
	public static object GetObject(string path)
	{
		var (obj, type) = GetObjectRaw(path);
		if (obj == null)
		{
			return DumpStatic(type);
		}
		return Dump(obj);
	}

	private static (object obj, Type type) GetObjectRaw(string path)
	{
		string[] array = path.Split('.');
		object obj = null;
		int num;
		Type type;
		if (!RootFinders.Finders.TryGetValue(array[0], out var value))
		{
			(type, num) = GetType(array);
		}
		else
		{
			(object instance, Type type, int used) tuple2 = value(array.Skip(1).ToArray());
			obj = tuple2.instance;
			type = tuple2.type;
			num = tuple2.used;
			num++;
		}
		while (num < array.Length)
		{
			Match match = _indexer.Match(array[num]);
			string fieldName = (match.Success ? match.Groups[1].Value : array[num]);
			num++;
			var (text, alias) = Aliases.KnownAliases.FirstOrDefault(((string prefix, Aliases.Alias alias) v) => fieldName.StartsWith(v.prefix));
			if (alias != null)
			{
				fieldName = fieldName.Substring(text.Length);
				(obj, type, num) = alias(obj, type, num, fieldName, array);
			}
			else
			{
				obj = GetFieldOrPropertyValue(type, obj, fieldName);
			}
			if (match.Success)
			{
				string indexStr = match.Groups[2].Value;
				var (text2, indexer) = CustomIndexers.Known.FirstOrDefault(((string prefix, CustomIndexers.Indexer alias) v) => indexStr.StartsWith(v.prefix));
				if (indexer != null)
				{
					indexStr = indexStr.Substring(text2.Length);
				}
				else
				{
					indexer = Index;
				}
				(obj, type) = indexer(obj, type, indexStr);
			}
			if (obj == null)
			{
				throw new NullReferenceException("Value at path " + string.Join(".", array.Take(num)) + " is null");
			}
			type = obj.GetType();
		}
		return (obj: obj, type: type);
	}

	private static (Type type, int idx) GetType(string[] parts)
	{
		string text = parts[0];
		Type typeByFullName = GetTypeByFullName(text);
		int num = 1;
		while (typeByFullName == null && num < parts.Length)
		{
			text = text + "." + parts[num];
			typeByFullName = GetTypeByFullName(text);
			num++;
		}
		if (typeByFullName == null)
		{
			throw new TypeAccessException("Cant get type from path " + text);
		}
		return (type: typeByFullName, idx: num);
	}

	private static string Dump(object obj)
	{
		if (obj is IEnumerable obj2)
		{
			return DumpCollection(obj2);
		}
		if (obj.GetType().IsPrimitive)
		{
			return obj.ToString();
		}
		if (obj is string result)
		{
			return result;
		}
		return DumpObject(obj);
	}

	private static string DumpCollection(IEnumerable obj)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Collection {0} {1}", obj.GetType().TypeName(), obj).AppendLine();
		stringBuilder.AppendLine("[");
		foreach (var (num, arg) in Enumerable.Range(0, 20).Zip(obj.Cast<object>(), (int item, object idx) => (item: item, idx: idx)))
		{
			stringBuilder.AppendFormat("[{0}]: {1}", num, arg).AppendLine();
		}
		stringBuilder.AppendLine("]");
		return stringBuilder.ToString();
	}

	private static string DumpObject(object obj)
	{
		IEnumerable<(string Name, string)> first = from v in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
			where !v.CustomAttributes.Any((CustomAttributeData attr) => attr.AttributeType == typeof(CompilerGeneratedAttribute))
			select (Name: v.Name, v.GetValue(obj)?.ToString() ?? "null");
		IEnumerable<(string, string)> second = from v in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
			where !v.CustomAttributes.Any((CustomAttributeData attr) => attr.AttributeType == typeof(CompilerGeneratedAttribute))
			select (Name: v.Name, Value: GetValueSafe(v, obj));
		IOrderedEnumerable<(string, string)> orderedEnumerable = from v in first.Concat<(string, string)>(second)
			orderby v.Name
			select v;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Object {0} {1}", obj.GetType().TypeName(), obj).AppendLine();
		stringBuilder.AppendLine("{");
		foreach (var (arg, arg2) in orderedEnumerable)
		{
			stringBuilder.AppendFormat("    {0}: {1}", arg, arg2).AppendLine();
		}
		stringBuilder.AppendLine("}");
		return stringBuilder.ToString();
	}

	private static string DumpStatic(Type type)
	{
		IEnumerable<(string Name, string)> first = from v in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
			select (Name: v.Name, v.GetValue(null)?.ToString() ?? "null");
		IEnumerable<(string, string)> second = from v in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
			select (Name: v.Name, Value: GetValueSafe(v, null));
		IOrderedEnumerable<(string, string)> orderedEnumerable = from v in first.Concat<(string, string)>(second)
			orderby v.Name
			select v;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Type {0}", type.TypeName()).AppendLine();
		stringBuilder.AppendLine("{");
		foreach (var (arg, arg2) in orderedEnumerable)
		{
			stringBuilder.AppendFormat("    {0}: {1}", arg, arg2).AppendLine();
		}
		stringBuilder.AppendLine("}");
		return stringBuilder.ToString();
	}

	private static string GetValueSafe(PropertyInfo prop, object obj)
	{
		try
		{
			return prop.GetValue(obj)?.ToString() ?? "null";
		}
		catch (Exception ex)
		{
			return "Exception: " + ex.Message;
		}
	}

	private static Type GetTypeByFullName(string name)
	{
		return (from v in AppDomain.CurrentDomain.GetAssembliesSafe()
			select v.GetTypeSafe(name) into v
			where v != null
			select v).ToArray().SingleOrDefault();
	}

	private static object GetFieldOrPropertyValue(Type type, object instance, string name)
	{
		PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			return property.GetValue(instance);
		}
		FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			return field.GetValue(instance);
		}
		throw new FieldAccessException($"Cannot find field or property {name} in type {type}");
	}

	private static (object obj, Type type) Index(object instance, Type type, string idx)
	{
		if (instance is IEnumerable source && int.TryParse(idx, out var result))
		{
			object obj = source.Cast<object>().Skip(result).First();
			return (obj: obj, type: obj.GetType());
		}
		PropertyInfo property = instance.GetType().GetProperty("Item", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (property.GetIndexParameters().Length == 0)
		{
			throw new TypeAccessException("Cant find index property of class " + type.TypeName());
		}
		if (property.GetIndexParameters().Length > 1)
		{
			throw new TypeAccessException("Index properties with more than one parameter are unsupported. Class " + type.TypeName());
		}
		if (property.GetIndexParameters()[0].ParameterType == typeof(string))
		{
			object value = property.GetValue(instance, new object[1] { idx });
			return (obj: value, type: value.GetType());
		}
		object obj2 = Convert.ChangeType(idx, property.GetIndexParameters()[0].ParameterType);
		object value2 = property.GetValue(instance, new object[1] { obj2 });
		return (obj: value2, type: value2.GetType());
	}
}
