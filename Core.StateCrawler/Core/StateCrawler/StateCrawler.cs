using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Core.StateCrawler;

public static class StateCrawler
{
	public struct ExpandedChildren
	{
		public string ChildName;

		public List<ExpandedChildren> Children;
	}

	public struct Node
	{
		public string Name;

		public string Value;

		public string Type;

		public bool IsEnumerable;

		public string ExceptionMessage;

		public string ExceptionStackTrace;

		public bool HasChildren;

		public List<Node> Children;

		[DefaultValue(-1)]
		public int Index;

		[DefaultValue(-1)]
		public int Count;

		public Node(string name)
		{
			Name = name;
			Index = -1;
			Count = -1;
			Value = null;
			Type = null;
			ExceptionMessage = null;
			ExceptionStackTrace = null;
			HasChildren = false;
			IsEnumerable = false;
			Children = new List<Node>();
		}
	}

	private static readonly Regex ArrayIndexer;

	private static readonly Dictionary<string, Type[]> AllTypes;

	static StateCrawler()
	{
		ArrayIndexer = new Regex("(\\w+)\\[([0-9]+)\\]");
		AllTypes = (from i in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly i) => i.GetTypes())
			group i by i.Name).ToDictionary((IGrouping<string, Type> i) => i.Key, (IGrouping<string, Type> i) => i.ToArray());
	}

	public static Node GetState(string rootPath, List<ExpandedChildren> expandedChildrenList)
	{
		if (string.IsNullOrEmpty(rootPath))
		{
			return default(Node);
		}
		try
		{
			object rootObject = GetRootObject(rootPath);
			Node result;
			if (rootObject == null)
			{
				Node node = new Node(rootPath);
				node.ExceptionMessage = "Can't resolve object path";
				result = node;
			}
			else
			{
				result = GetObjectState(rootPath, rootObject, isExpanded: true, expandedChildrenList);
			}
			return result;
		}
		catch (Exception ex)
		{
			Node result2 = new Node(rootPath);
			result2.ExceptionMessage = ex.Message;
			result2.ExceptionStackTrace = ex.StackTrace;
			return result2;
		}
	}

	private static object GetRootObject(string path)
	{
		string[] array = path.Split('.');
		object obj = null;
		string[] array2 = array;
		foreach (string text in array2)
		{
			object obj2 = ((obj == null) ? GetType(text) : ((!(obj is Type type)) ? GetMemberValue(obj, text) : (type.GetNestedType(text, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy) ?? GetMemberValue(type, text))));
			obj = obj2;
		}
		return obj;
		static Type GetType(string name)
		{
			if (!AllTypes.TryGetValue(name, out var value))
			{
				return Type.GetType(name);
			}
			return value[0];
		}
	}

	private static object GetMemberValue(object obj, string memberName)
	{
		if (obj == null)
		{
			return null;
		}
		Match match = ArrayIndexer.Match(memberName);
		if (match.Success)
		{
			string text = match.Groups[1].ToString();
			int num = int.Parse(match.Groups[2].ToString());
			IEnumerable obj2 = (GetMemberValue(obj, text) as IEnumerable) ?? throw new Exception("'" + text + "' isn't enumerable and can't be indexed");
			int num2 = 0;
			foreach (object item in obj2)
			{
				if (num2++ == num)
				{
					return item;
				}
			}
			throw new Exception($"Index out of range: '{text}', index: {num}, count: {num2}");
		}
		Type obj3 = (obj as Type) ?? obj.GetType();
		object obj4 = ((obj is Type) ? null : obj);
		MemberInfo memberInfo = obj3.GetMember(memberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty).FirstOrDefault();
		if (!(memberInfo is FieldInfo fieldInfo))
		{
			if (memberInfo is PropertyInfo propertyInfo)
			{
				return propertyInfo.GetValue(obj4);
			}
			throw new Exception("Can't resolve name '" + memberName + "'");
		}
		return fieldInfo.GetValue(obj4);
	}

	private static Node GetObjectState(string name, object obj, bool isExpanded, List<ExpandedChildren> expandedChildrenList)
	{
		try
		{
			return GetObjectStateInternal(name, obj, isExpanded, expandedChildrenList);
		}
		catch (Exception ex)
		{
			Node result = new Node(name);
			result.ExceptionMessage = ex.Message;
			result.ExceptionStackTrace = ex.StackTrace;
			return result;
		}
	}

	private static Node GetObjectStateInternal(string name, object obj, bool isExpanded, [CanBeNull] List<ExpandedChildren> expandedChildrenList)
	{
		string value = obj?.ToString();
		string type = obj?.GetType().FullName;
		IEnumerable<MemberInfo> members = GetMembers(obj);
		Node node = new Node(name);
		node.Value = value;
		node.Type = type;
		node.HasChildren = members.Any();
		node.IsEnumerable = obj is IEnumerable;
		Node result = node;
		IEnumerable enumerable = obj as IEnumerable;
		if (enumerable != null && !(obj is string))
		{
			result.Count = enumerable.Cast<object>().Count();
		}
		if (!isExpanded || obj == null)
		{
			return result;
		}
		bool flag = IsBuiltInEnumerableType(obj.GetType());
		if (enumerable != null)
		{
			if (name == "$enumerable" || flag)
			{
				int num = 0;
				foreach (object item2 in enumerable)
				{
					string text = $"{num}";
					ExtractExpandedChildInfo(text, out var expanded2, out var children2);
					Node objectState = GetObjectState(text, item2, expanded2, children2);
					objectState.Index = num;
					result.Children.Add(objectState);
					num++;
				}
			}
			else
			{
				ExtractExpandedChildInfo("$enumerable", out var expanded3, out var children3);
				Node objectState2 = GetObjectState("$enumerable", obj, expanded3, children3);
				result.Children.Add(objectState2);
			}
		}
		if (!flag && name != "$enumerable")
		{
			foreach (MemberInfo item3 in members)
			{
				if (TryGetMemberValue(obj, item3, out var value2, out var exception))
				{
					ExtractExpandedChildInfo(item3.Name, out var expanded4, out var children4);
					Node objectState3 = GetObjectState(item3.Name, value2, expanded4, children4);
					result.Children.Add(objectState3);
				}
				else
				{
					node = new Node(item3.Name);
					node.ExceptionMessage = exception.Message;
					node.ExceptionStackTrace = exception.StackTrace;
					Node item = node;
					result.Children.Add(item);
				}
			}
		}
		return result;
		void ExtractExpandedChildInfo(string childName, out bool expanded, [CanBeNull] out List<ExpandedChildren> children)
		{
			int num2 = expandedChildrenList?.FindIndex((ExpandedChildren i) => i.ChildName == childName) ?? (-1);
			expanded = num2 >= 0;
			children = ((num2 < 0) ? null : expandedChildrenList?[num2].Children);
		}
		static bool IsBuiltInEnumerableType(Type enumerableType)
		{
			if (!enumerableType.IsArray)
			{
				if (enumerableType.IsGenericType)
				{
					Type genericTypeDefinition = enumerableType.GetGenericTypeDefinition();
					if ((object)genericTypeDefinition != null)
					{
						if (!(genericTypeDefinition == typeof(List<>)) && !(genericTypeDefinition == typeof(HashSet<>)))
						{
							return genericTypeDefinition == typeof(Dictionary<, >);
						}
						return true;
					}
				}
				return false;
			}
			return true;
		}
	}

	private static bool TryGetMemberValue(object target, MemberInfo member, out object value, out Exception exception)
	{
		try
		{
			target = ((target is Type) ? null : target);
			object obj = ((member is FieldInfo fieldInfo) ? fieldInfo.GetValue(target) : ((!(member is PropertyInfo propertyInfo)) ? null : propertyInfo.GetValue(target)));
			value = obj;
			exception = null;
			return true;
		}
		catch (Exception ex)
		{
			value = null;
			exception = ex.InnerException ?? ex;
			return false;
		}
	}

	private static IEnumerable<MemberInfo> GetMembers(object obj)
	{
		if (obj == null || obj.GetType().IsPrimitive || obj is string)
		{
			return Enumerable.Empty<MemberInfo>();
		}
		BindingFlags bindingAttr = ((obj is Type) ? (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty) : (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty));
		Type type = (obj as Type) ?? obj.GetType();
		return from i in type.GetFields(bindingAttr).Cast<MemberInfo>().Concat(from i in type.GetProperties(bindingAttr)
				where i.GetIndexParameters().Length < 1
				select i)
			where i.CustomAttributes.All((CustomAttributeData attr) => attr.AttributeType != typeof(CompilerGeneratedAttribute))
			select i;
	}
}
