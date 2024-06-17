using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Blueprints.JsonSystem.BinaryFormat;

public class ReflectionBasedSerializer
{
	private PrimitiveSerializer m_Primitive;

	private SimpleBlueprint m_BlueprintBeingRead;

	private Dictionary<Type, List<FieldInfo>> m_FieldsCache = new Dictionary<Type, List<FieldInfo>>();

	public ReflectionBasedSerializer(PrimitiveSerializer s)
	{
		m_Primitive = s;
	}

	public void Blueprint(ref SimpleBlueprint bp)
	{
		object obj = bp;
		GenericObject(ref obj);
		bp = obj as SimpleBlueprint;
		if (bp != null)
		{
			m_Primitive.String(ref bp.name);
			m_Primitive.String(ref bp.AssetGuid);
		}
	}

	private void GenericObject(ref object obj, Type fieldType = null)
	{
		bool flag = (obj == null && fieldType == null) || IsIdentifiedType(obj?.GetType() ?? fieldType);
		if (m_Primitive.WriteMode)
		{
			if (flag)
			{
				m_Primitive.WriteType(obj?.GetType());
			}
			if (obj == null && fieldType != null && !flag)
			{
				obj = Activator.CreateInstance(fieldType);
			}
			if (obj == null)
			{
				return;
			}
			{
				foreach (MemberInfo unitySerializedField in BlueprintFieldsTraverser.GetUnitySerializedFields(obj.GetType()))
				{
					WriteField((FieldInfo)unitySerializedField, obj);
				}
				return;
			}
		}
		Type type = fieldType;
		if (flag)
		{
			type = m_Primitive.ReadType();
		}
		if (type == null)
		{
			obj = null;
			return;
		}
		obj = CreateObject(type);
		HandleBlueprintBacklinks(obj);
		foreach (MemberInfo unitySerializedField2 in BlueprintFieldsTraverser.GetUnitySerializedFields(type))
		{
			try
			{
				ReadField((FieldInfo)unitySerializedField2, obj);
			}
			catch (Exception)
			{
				PFLog.Default.Error("Reading " + unitySerializedField2.Name + " in " + unitySerializedField2.DeclaringType.Name);
				throw;
			}
		}
	}

	private static object CreateObject(Type type)
	{
		return type.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? FormatterServices.GetUninitializedObject(type);
	}

	private void HandleBlueprintBacklinks(object obj)
	{
		if (obj is SimpleBlueprint blueprintBeingRead)
		{
			m_BlueprintBeingRead = blueprintBeingRead;
		}
		if (obj is BlueprintComponent blueprintComponent)
		{
			blueprintComponent.OwnerBlueprint = m_BlueprintBeingRead as BlueprintScriptableObject;
		}
		if (obj is Element e)
		{
			m_BlueprintBeingRead.AddToElementsList(e);
		}
	}

	public IEnumerable<MemberInfo> GetUnitySerializedFields(Type type)
	{
		if (!m_FieldsCache.TryGetValue(type, out var value))
		{
			value = (from f in FieldsContractResolver.GetUnitySerializedFields(type)
				where !f.HasAttribute<JsonIgnoreAttribute>()
				where !f.HasAttribute<ExcludeFieldFromBuildAttribute>()
				select f).Cast<FieldInfo>().ToList();
			m_FieldsCache[type] = value;
		}
		return value;
	}

	private void WriteField(FieldInfo field, object obj)
	{
		Type fieldType = field.FieldType;
		if (fieldType == typeof(int))
		{
			int i = (int)field.GetValue(obj);
			m_Primitive.Int(ref i);
		}
		else if (fieldType == typeof(uint))
		{
			uint i2 = (uint)field.GetValue(obj);
			m_Primitive.UInt(ref i2);
		}
		else if (fieldType == typeof(float))
		{
			float f = (float)field.GetValue(obj);
			m_Primitive.Float(ref f);
		}
		else if (fieldType == typeof(double))
		{
			double f2 = (double)field.GetValue(obj);
			m_Primitive.Double(ref f2);
		}
		else if (fieldType == typeof(long))
		{
			long i3 = (long)field.GetValue(obj);
			m_Primitive.Long(ref i3);
		}
		else if (fieldType == typeof(ulong))
		{
			ulong i4 = (ulong)field.GetValue(obj);
			m_Primitive.ULong(ref i4);
		}
		else if (fieldType == typeof(string))
		{
			string i5 = (string)field.GetValue(obj);
			m_Primitive.String(ref i5);
		}
		else if (fieldType == typeof(bool))
		{
			bool i6 = (bool)field.GetValue(obj);
			m_Primitive.Bool(ref i6);
		}
		else if (fieldType.IsEnum)
		{
			EnsureEnumIsInt(fieldType, field);
			int i7 = Convert.ToInt32(field.GetValue(obj));
			m_Primitive.Int(ref i7);
		}
		else if (fieldType == typeof(Color))
		{
			Color v = (Color)field.GetValue(obj);
			m_Primitive.Color(ref v);
		}
		else if (fieldType == typeof(Color32))
		{
			Color32 v2 = (Color32)field.GetValue(obj);
			m_Primitive.Color32(ref v2);
		}
		else if (fieldType == typeof(Vector2))
		{
			Vector2 v3 = (Vector2)field.GetValue(obj);
			m_Primitive.Vector(ref v3);
		}
		else if (fieldType == typeof(Vector3))
		{
			Vector3 v4 = (Vector3)field.GetValue(obj);
			m_Primitive.Vector(ref v4);
		}
		else if (fieldType == typeof(Vector4))
		{
			Vector4 v5 = (Vector4)field.GetValue(obj);
			m_Primitive.Vector(ref v5);
		}
		else if (fieldType == typeof(Vector2Int))
		{
			Vector2Int v6 = (Vector2Int)field.GetValue(obj);
			m_Primitive.VectorInt(ref v6);
		}
		else if (fieldType == typeof(Gradient))
		{
			Gradient cv = (Gradient)field.GetValue(obj);
			m_Primitive.Gradient(ref cv);
		}
		else if (fieldType == typeof(AnimationCurve))
		{
			AnimationCurve cv2 = (AnimationCurve)field.GetValue(obj);
			m_Primitive.AnimationCurve(ref cv2);
		}
		else if (fieldType == typeof(ColorBlock))
		{
			ColorBlock cb = (ColorBlock)field.GetValue(obj);
			m_Primitive.ColorBlock(ref cb);
		}
		else if (fieldType.IsOrSubclassOf<UnityEngine.Object>())
		{
			UnityEngine.Object obj2 = (UnityEngine.Object)field.GetValue(obj);
			m_Primitive.UnityObject(ref obj2);
		}
		else if (fieldType == typeof(int[]))
		{
			int[] a = (int[])field.GetValue(obj);
			m_Primitive.IntArray(ref a);
		}
		else if (fieldType == typeof(uint[]))
		{
			uint[] a2 = (uint[])field.GetValue(obj);
			m_Primitive.UIntArray(ref a2);
		}
		else if (fieldType == typeof(float[]))
		{
			float[] a3 = (float[])field.GetValue(obj);
			m_Primitive.FloatArray(ref a3);
		}
		else if (fieldType == typeof(long[]))
		{
			long[] a4 = (long[])field.GetValue(obj);
			m_Primitive.LongArray(ref a4);
		}
		else if (fieldType == typeof(ulong[]))
		{
			ulong[] a5 = (ulong[])field.GetValue(obj);
			m_Primitive.ULongArray(ref a5);
		}
		else if (fieldType == typeof(string[]))
		{
			string[] a6 = (string[])field.GetValue(obj);
			m_Primitive.StringArray(ref a6);
		}
		else if (fieldType == typeof(bool[]))
		{
			bool[] a7 = (bool[])field.GetValue(obj);
			m_Primitive.BoolArray(ref a7);
		}
		else if (fieldType.IsArray && fieldType.GetElementType().IsEnum)
		{
			EnsureEnumIsInt(fieldType.GetElementType(), field);
			Array array = (Array)field.GetValue(obj);
			int[] a8 = ((array != null) ? array.Cast<object>().Select(Convert.ToInt32).ToArray() : Array.Empty<int>());
			m_Primitive.IntArray(ref a8);
		}
		else if (fieldType == typeof(Color[]))
		{
			Color[] cs = (Color[])field.GetValue(obj);
			m_Primitive.ColorArray(ref cs);
		}
		else if (fieldType == typeof(Color32[]))
		{
			Color32[] cs2 = (Color32[])field.GetValue(obj);
			m_Primitive.Color32Array(ref cs2);
		}
		else if (fieldType == typeof(Vector2[]))
		{
			Vector2[] v7 = (Vector2[])field.GetValue(obj);
			m_Primitive.VectorArray(ref v7);
		}
		else if (fieldType == typeof(Vector3[]))
		{
			Vector3[] v8 = (Vector3[])field.GetValue(obj);
			m_Primitive.VectorArray(ref v8);
		}
		else if (fieldType == typeof(Vector4[]))
		{
			Vector4[] v9 = (Vector4[])field.GetValue(obj);
			m_Primitive.VectorArray(ref v9);
		}
		else if (fieldType == typeof(Vector2Int[]))
		{
			Vector2Int[] v10 = (Vector2Int[])field.GetValue(obj);
			m_Primitive.VectorIntArray(ref v10);
		}
		else
		{
			if (fieldType.IsArray && fieldType.GetElementType().IsOrSubclassOf<UnityEngine.Object>())
			{
				Array array2 = (Array)field.GetValue(obj);
				int i8 = array2?.Length ?? 0;
				m_Primitive.Int(ref i8);
				if (array2 == null)
				{
					return;
				}
				{
					foreach (object item in array2)
					{
						UnityEngine.Object obj3 = item as UnityEngine.Object;
						m_Primitive.UnityObject(ref obj3);
					}
					return;
				}
			}
			if (fieldType.IsListOf<string>())
			{
				List<string> a9 = (List<string>)field.GetValue(obj);
				m_Primitive.StringList(ref a9);
			}
			else if (fieldType.IsListOf<int>())
			{
				List<int> a10 = (List<int>)field.GetValue(obj);
				m_Primitive.IntList(ref a10);
			}
			else if (fieldType.IsList() && fieldType.GetGenericArguments()[0].IsEnum)
			{
				EnsureEnumIsInt(fieldType.GetGenericArguments()[0], field);
				int[] a11 = (((IList)field.GetValue(obj))?.Cast<int>()).EmptyIfNull().ToArray();
				m_Primitive.IntArray(ref a11);
			}
			else if (fieldType.IsListOf<Color32>())
			{
				List<Color32> cs3 = (List<Color32>)field.GetValue(obj);
				m_Primitive.Color32List(ref cs3);
			}
			else if (fieldType.IsListOf<Vector2Int>())
			{
				List<Vector2Int> vs = (List<Vector2Int>)field.GetValue(obj);
				m_Primitive.VectorIntList(ref vs);
			}
			else
			{
				if (fieldType.IsList() && fieldType.GenericTypeArguments[0].IsOrSubclassOf<UnityEngine.Object>())
				{
					IList list = (IList)field.GetValue(obj);
					int i9 = list?.Count ?? 0;
					m_Primitive.Int(ref i9);
					if (list == null)
					{
						return;
					}
					{
						foreach (object item2 in list)
						{
							UnityEngine.Object obj4 = item2 as UnityEngine.Object;
							m_Primitive.UnityObject(ref obj4);
						}
						return;
					}
				}
				if (fieldType.IsArray)
				{
					Type elementType = fieldType.GetElementType();
					Array array3 = (Array)field.GetValue(obj);
					int i10 = array3?.Length ?? 0;
					m_Primitive.Int(ref i10);
					if (array3 == null)
					{
						return;
					}
					{
						foreach (object item3 in array3)
						{
							object obj5 = item3;
							GenericObject(ref obj5, elementType);
						}
						return;
					}
				}
				if (fieldType.IsList())
				{
					Type fieldType2 = fieldType.GenericTypeArguments[0];
					IList list2 = (IList)field.GetValue(obj);
					int i11 = list2?.Count ?? 0;
					m_Primitive.Int(ref i11);
					if (list2 == null)
					{
						return;
					}
					{
						foreach (object item4 in list2)
						{
							object obj6 = item4;
							GenericObject(ref obj6, fieldType2);
						}
						return;
					}
				}
				if (IsIdentifiedType(fieldType) || fieldType.HasAttribute<SerializableAttribute>())
				{
					object obj7 = field.GetValue(obj);
					GenericObject(ref obj7, fieldType);
					return;
				}
				PFLog.Default.Error("Cannot serialize field " + field.Name + " of " + field.DeclaringType.Name + ": unrecognized field type");
			}
		}
	}

	private void ReadField(FieldInfo field, object obj)
	{
		if (field.FieldType == typeof(GUIStyle))
		{
			field.SetValue(obj, new GUIStyle());
			return;
		}
		Type fieldType = field.FieldType;
		if (fieldType == typeof(int))
		{
			int i = 0;
			m_Primitive.Int(ref i);
			field.SetValue(obj, i);
		}
		else if (fieldType == typeof(uint))
		{
			uint i2 = 0u;
			m_Primitive.UInt(ref i2);
			field.SetValue(obj, i2);
		}
		else if (fieldType == typeof(float))
		{
			float f = 0f;
			m_Primitive.Float(ref f);
			field.SetValue(obj, f);
		}
		else if (fieldType == typeof(double))
		{
			double f2 = 0.0;
			m_Primitive.Double(ref f2);
			field.SetValue(obj, f2);
		}
		else if (fieldType == typeof(long))
		{
			long i3 = 0L;
			m_Primitive.Long(ref i3);
			field.SetValue(obj, i3);
		}
		else if (fieldType == typeof(ulong))
		{
			ulong i4 = 0uL;
			m_Primitive.ULong(ref i4);
			field.SetValue(obj, i4);
		}
		else if (fieldType == typeof(string))
		{
			string i5 = null;
			m_Primitive.String(ref i5);
			field.SetValue(obj, i5);
		}
		else if (fieldType == typeof(bool))
		{
			bool i6 = false;
			m_Primitive.Bool(ref i6);
			field.SetValue(obj, i6);
		}
		else if (fieldType.IsEnum)
		{
			int i7 = 0;
			m_Primitive.Int(ref i7);
			field.SetValue(obj, Enum.ToObject(fieldType, i7));
		}
		else if (fieldType == typeof(Color))
		{
			Color v = default(Color);
			m_Primitive.Color(ref v);
			field.SetValue(obj, v);
		}
		else if (fieldType == typeof(Color32))
		{
			Color32 v2 = default(Color32);
			m_Primitive.Color32(ref v2);
			field.SetValue(obj, v2);
		}
		else if (fieldType == typeof(Vector2))
		{
			Vector2 v3 = default(Vector2);
			m_Primitive.Vector(ref v3);
			field.SetValue(obj, v3);
		}
		else if (fieldType == typeof(Vector3))
		{
			Vector3 v4 = default(Vector3);
			m_Primitive.Vector(ref v4);
			field.SetValue(obj, v4);
		}
		else if (fieldType == typeof(Vector4))
		{
			Vector4 v5 = default(Vector4);
			m_Primitive.Vector(ref v5);
			field.SetValue(obj, v5);
		}
		else if (fieldType == typeof(Vector2Int))
		{
			Vector2Int v6 = default(Vector2Int);
			m_Primitive.VectorInt(ref v6);
			field.SetValue(obj, v6);
		}
		else if (fieldType == typeof(Gradient))
		{
			Gradient cv = null;
			m_Primitive.Gradient(ref cv);
			field.SetValue(obj, cv);
		}
		else if (fieldType == typeof(AnimationCurve))
		{
			AnimationCurve cv2 = null;
			m_Primitive.AnimationCurve(ref cv2);
			field.SetValue(obj, cv2);
		}
		else if (fieldType == typeof(ColorBlock))
		{
			ColorBlock cb = default(ColorBlock);
			m_Primitive.ColorBlock(ref cb);
			field.SetValue(obj, cb);
		}
		else if (fieldType.IsOrSubclassOf<UnityEngine.Object>())
		{
			UnityEngine.Object obj2 = null;
			m_Primitive.UnityObject(ref obj2);
			field.SetValue(obj, obj2);
		}
		else if (fieldType == typeof(int[]))
		{
			int[] a = null;
			m_Primitive.IntArray(ref a);
			field.SetValue(obj, a);
		}
		else if (fieldType == typeof(uint[]))
		{
			uint[] a2 = null;
			m_Primitive.UIntArray(ref a2);
			field.SetValue(obj, a2);
		}
		else if (fieldType == typeof(float[]))
		{
			float[] a3 = null;
			m_Primitive.FloatArray(ref a3);
			field.SetValue(obj, a3);
		}
		else if (fieldType == typeof(long[]))
		{
			long[] a4 = null;
			m_Primitive.LongArray(ref a4);
			field.SetValue(obj, a4);
		}
		else if (fieldType == typeof(ulong[]))
		{
			ulong[] a5 = null;
			m_Primitive.ULongArray(ref a5);
			field.SetValue(obj, a5);
		}
		else if (fieldType == typeof(string[]))
		{
			string[] a6 = null;
			m_Primitive.StringArray(ref a6);
			field.SetValue(obj, a6);
		}
		else if (fieldType == typeof(bool[]))
		{
			bool[] a7 = null;
			m_Primitive.BoolArray(ref a7);
			field.SetValue(obj, a7);
		}
		else if (fieldType.IsArray && fieldType.GetElementType().IsEnum)
		{
			int[] a8 = null;
			m_Primitive.IntArray(ref a8);
			Array array = Array.CreateInstance(fieldType.GetElementType(), a8.Length);
			for (int j = 0; j < a8.Length; j++)
			{
				array.SetValue(Enum.ToObject(fieldType.GetElementType(), a8[j]), j);
			}
			field.SetValue(obj, array);
		}
		else if (fieldType == typeof(Color[]))
		{
			Color[] cs = null;
			m_Primitive.ColorArray(ref cs);
			field.SetValue(obj, cs);
		}
		else if (fieldType == typeof(Color32[]))
		{
			Color32[] cs2 = null;
			m_Primitive.Color32Array(ref cs2);
			field.SetValue(obj, cs2);
		}
		else if (fieldType == typeof(Vector2[]))
		{
			Vector2[] v7 = null;
			m_Primitive.VectorArray(ref v7);
			field.SetValue(obj, v7);
		}
		else if (fieldType == typeof(Vector3[]))
		{
			Vector3[] v8 = null;
			m_Primitive.VectorArray(ref v8);
			field.SetValue(obj, v8);
		}
		else if (fieldType == typeof(Vector4[]))
		{
			Vector4[] v9 = null;
			m_Primitive.VectorArray(ref v9);
			field.SetValue(obj, v9);
		}
		else if (fieldType == typeof(Vector2Int[]))
		{
			Vector2Int[] v10 = null;
			m_Primitive.VectorIntArray(ref v10);
			field.SetValue(obj, v10);
		}
		else if (fieldType.IsArray && fieldType.GetElementType().IsOrSubclassOf<UnityEngine.Object>())
		{
			int i8 = 0;
			m_Primitive.Int(ref i8);
			Array array2 = Array.CreateInstance(fieldType.GetElementType(), i8);
			for (int k = 0; k < i8; k++)
			{
				UnityEngine.Object obj3 = null;
				m_Primitive.UnityObject(ref obj3);
				array2.SetValue(obj3, k);
			}
			field.SetValue(obj, array2);
		}
		else if (fieldType.IsListOf<string>())
		{
			List<string> a9 = null;
			m_Primitive.StringList(ref a9);
			field.SetValue(obj, a9);
		}
		else if (fieldType.IsListOf<int>())
		{
			List<int> a10 = null;
			m_Primitive.IntList(ref a10);
			field.SetValue(obj, a10);
		}
		else if (fieldType.IsList() && fieldType.GetGenericArguments()[0].IsEnum)
		{
			int[] a11 = null;
			m_Primitive.IntArray(ref a11);
			IList list = (IList)Activator.CreateInstance(fieldType);
			int[] array3 = a11;
			foreach (int value in array3)
			{
				list.Add(Enum.ToObject(fieldType.GetGenericArguments()[0], value));
			}
			field.SetValue(obj, list);
		}
		else if (fieldType.IsListOf<Color32>())
		{
			List<Color32> cs3 = null;
			m_Primitive.Color32List(ref cs3);
			field.SetValue(obj, cs3);
		}
		else if (fieldType.IsListOf<Vector2Int>())
		{
			List<Vector2Int> vs = null;
			m_Primitive.VectorIntList(ref vs);
			field.SetValue(obj, vs);
		}
		else if (fieldType.IsList() && fieldType.GenericTypeArguments[0].IsOrSubclassOf<UnityEngine.Object>())
		{
			int i9 = 0;
			m_Primitive.Int(ref i9);
			IList list2 = (IList)Activator.CreateInstance(fieldType);
			for (int m = 0; m < i9; m++)
			{
				UnityEngine.Object obj4 = null;
				m_Primitive.UnityObject(ref obj4);
				list2.Add(obj4);
			}
			field.SetValue(obj, list2);
		}
		else if (fieldType.IsArray)
		{
			int i10 = 0;
			m_Primitive.Int(ref i10);
			Array array4 = Array.CreateInstance(fieldType.GetElementType(), i10);
			for (int n = 0; n < i10; n++)
			{
				object obj5 = null;
				GenericObject(ref obj5, fieldType.GetElementType());
				array4.SetValue(obj5, n);
			}
			field.SetValue(obj, array4);
		}
		else if (fieldType.IsList())
		{
			int i11 = 0;
			m_Primitive.Int(ref i11);
			IList list3 = (IList)Activator.CreateInstance(fieldType);
			for (int num = 0; num < i11; num++)
			{
				object obj6 = null;
				GenericObject(ref obj6, fieldType.GenericTypeArguments[0]);
				list3.Add(obj6);
			}
			field.SetValue(obj, list3);
		}
		else if (IsIdentifiedType(fieldType) || fieldType.HasAttribute<SerializableAttribute>())
		{
			object obj7 = null;
			GenericObject(ref obj7, fieldType);
			field.SetValue(obj, obj7);
		}
		else
		{
			PFLog.Default.Error("Cannot serialize field " + field.Name + " of " + field.DeclaringType.Name + ": unrecognized field type");
		}
	}

	private static void EnsureEnumIsInt(Type e, FieldInfo field)
	{
		if (!e.IsEnum || !(Enum.GetUnderlyingType(e) != typeof(int)))
		{
			return;
		}
		throw new NotImplementedException("Not an int-based enum in " + field.DeclaringType.Name + "." + field.Name);
	}

	private static bool IsIdentifiedType(Type t)
	{
		if (!t.IsOrSubclassOf<SimpleBlueprint>() && !t.IsOrSubclassOf<BlueprintComponent>())
		{
			return t.IsOrSubclassOf<Element>();
		}
		return true;
	}
}
