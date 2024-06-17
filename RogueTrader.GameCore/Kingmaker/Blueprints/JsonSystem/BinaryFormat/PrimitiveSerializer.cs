using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Logging;
using RogueTrader.SharedTypes;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Blueprints.JsonSystem.BinaryFormat;

public class PrimitiveSerializer
{
	private readonly BinaryReader m_Reader;

	private readonly BinaryWriter m_Writer;

	private readonly BlueprintReferencedAssets m_ReferencedAssets;

	private readonly byte[] m_GuidBuffer = new byte[16];

	public bool WriteMode { get; }

	public PrimitiveSerializer(BinaryWriter writer, BlueprintReferencedAssets referencedAssets)
	{
		m_Writer = writer;
		m_ReferencedAssets = referencedAssets;
		WriteMode = true;
	}

	public PrimitiveSerializer(BinaryReader reader, BlueprintReferencedAssets referencedAssets)
	{
		m_Reader = reader;
		m_ReferencedAssets = referencedAssets;
		WriteMode = false;
	}

	public void Enum<T>(ref T e) where T : struct, IConvertible
	{
		if (WriteMode)
		{
			m_Writer.Write(Convert.ToInt32(e));
		}
		else
		{
			e = (T)System.Enum.ToObject(typeof(T), m_Reader.ReadInt32());
		}
	}

	public void EnumArray<T>(ref T[] a) where T : struct, IConvertible
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Length);
			T[] array = a;
			foreach (T val in array)
			{
				m_Writer.Write(Convert.ToInt32(val));
			}
		}
		else
		{
			a = new T[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = (T)System.Enum.ToObject(typeof(T), m_Reader.ReadInt32());
			}
		}
	}

	public void EnumList<T>(ref List<T> a) where T : struct, IConvertible
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Count);
			{
				foreach (T item in a)
				{
					m_Writer.Write(Convert.ToInt32(item));
				}
				return;
			}
		}
		a = new List<T>();
		int num = m_Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			a.Add((T)System.Enum.ToObject(typeof(T), m_Reader.ReadInt32()));
		}
	}

	public void Int(ref int i)
	{
		if (WriteMode)
		{
			m_Writer.Write(i);
		}
		else
		{
			i = m_Reader.ReadInt32();
		}
	}

	public void IntArray(ref int[] a)
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Length);
			int[] array = a;
			foreach (int value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new int[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadInt32();
			}
		}
	}

	public void IntList(ref List<int> a)
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Count);
			{
				foreach (int item in a)
				{
					m_Writer.Write(item);
				}
				return;
			}
		}
		int num = m_Reader.ReadInt32();
		a = new List<int>(num);
		for (int i = 0; i < num; i++)
		{
			a.Add(m_Reader.ReadInt32());
		}
	}

	public void UInt(ref uint i)
	{
		if (WriteMode)
		{
			m_Writer.Write(i);
		}
		else
		{
			i = m_Reader.ReadUInt32();
		}
	}

	public void UIntArray(ref uint[] a)
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Length);
			uint[] array = a;
			foreach (uint value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new uint[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadUInt32();
			}
		}
	}

	public void Long(ref long i)
	{
		if (WriteMode)
		{
			m_Writer.Write(i);
		}
		else
		{
			i = m_Reader.ReadInt64();
		}
	}

	public void LongArray(ref long[] a)
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Length);
			long[] array = a;
			foreach (long value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new long[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadInt64();
			}
		}
	}

	public void ULong(ref ulong i)
	{
		if (WriteMode)
		{
			m_Writer.Write(i);
		}
		else
		{
			i = m_Reader.ReadUInt64();
		}
	}

	public void ULongArray(ref ulong[] a)
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Length);
			ulong[] array = a;
			foreach (ulong value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new ulong[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadUInt64();
			}
		}
	}

	public void Float(ref float f)
	{
		if (WriteMode)
		{
			m_Writer.Write(f);
		}
		else
		{
			f = m_Reader.ReadSingle();
		}
	}

	public void FloatArray(ref float[] a)
	{
		if (WriteMode)
		{
			a = a ?? new float[0];
			m_Writer.Write(a.Length);
			float[] array = a;
			foreach (float value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new float[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadSingle();
			}
		}
	}

	public void Double(ref double f)
	{
		if (WriteMode)
		{
			m_Writer.Write(f);
		}
		else
		{
			f = m_Reader.ReadDouble();
		}
	}

	public void Bool(ref bool i)
	{
		if (WriteMode)
		{
			m_Writer.Write(i);
		}
		else
		{
			i = m_Reader.ReadBoolean();
		}
	}

	public void BoolArray(ref bool[] a)
	{
		if (WriteMode)
		{
			m_Writer.Write(a.Length);
			bool[] array = a;
			foreach (bool value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new bool[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadBoolean();
			}
		}
	}

	public void String(ref string i)
	{
		if (WriteMode)
		{
			m_Writer.Write(i ?? "");
		}
		else
		{
			i = m_Reader.ReadString();
		}
	}

	public void StringArray(ref string[] a)
	{
		if (WriteMode)
		{
			a = a ?? new string[0];
			m_Writer.Write(a.Length);
			string[] array = a;
			foreach (string value in array)
			{
				m_Writer.Write(value);
			}
		}
		else
		{
			a = new string[m_Reader.ReadInt32()];
			for (int j = 0; j < a.Length; j++)
			{
				a[j] = m_Reader.ReadString();
			}
		}
	}

	public void StringList(ref List<string> a)
	{
		if (WriteMode)
		{
			a = a ?? new List<string>();
			m_Writer.Write(a.Count);
			{
				foreach (string item in a)
				{
					m_Writer.Write(item);
				}
				return;
			}
		}
		a = new List<string>();
		int num = m_Reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			a.Add(m_Reader.ReadString());
		}
	}

	public void Vector(ref Vector2 v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.x);
			m_Writer.Write(v.y);
		}
		else
		{
			v = new Vector2(m_Reader.ReadSingle(), m_Reader.ReadSingle());
		}
	}

	public void Vector(ref Vector3 v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.x);
			m_Writer.Write(v.y);
			m_Writer.Write(v.z);
		}
		else
		{
			v = new Vector3(m_Reader.ReadSingle(), m_Reader.ReadSingle(), m_Reader.ReadSingle());
		}
	}

	public void Vector(ref Vector4 v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.x);
			m_Writer.Write(v.y);
			m_Writer.Write(v.z);
			m_Writer.Write(v.w);
		}
		else
		{
			v = new Vector4(m_Reader.ReadSingle(), m_Reader.ReadSingle(), m_Reader.ReadSingle(), m_Reader.ReadSingle());
		}
	}

	public void VectorInt(ref Vector2Int v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.x);
			m_Writer.Write(v.y);
		}
		else
		{
			v = new Vector2Int(m_Reader.ReadInt32(), m_Reader.ReadInt32());
		}
	}

	public void VectorArray(ref Vector2[] v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.Length);
			for (int i = 0; i < v.Length; i++)
			{
				Vector(ref v[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		v = new Vector2[num];
		for (int j = 0; j < v.Length; j++)
		{
			Vector(ref v[j]);
		}
	}

	public void VectorArray(ref Vector3[] v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.Length);
			for (int i = 0; i < v.Length; i++)
			{
				Vector(ref v[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		v = new Vector3[num];
		for (int j = 0; j < v.Length; j++)
		{
			Vector(ref v[j]);
		}
	}

	public void VectorArray(ref Vector4[] v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.Length);
			for (int i = 0; i < v.Length; i++)
			{
				Vector(ref v[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		v = new Vector4[num];
		for (int j = 0; j < v.Length; j++)
		{
			Vector(ref v[j]);
		}
	}

	public void VectorIntArray(ref Vector2Int[] v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.Length);
			for (int i = 0; i < v.Length; i++)
			{
				VectorInt(ref v[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		v = new Vector2Int[num];
		for (int j = 0; j < v.Length; j++)
		{
			VectorInt(ref v[j]);
		}
	}

	public void Color(ref Color v)
	{
		if (WriteMode)
		{
			m_Writer.Write(v.r);
			m_Writer.Write(v.g);
			m_Writer.Write(v.b);
			m_Writer.Write(v.a);
		}
		else
		{
			v = new Color(m_Reader.ReadSingle(), m_Reader.ReadSingle(), m_Reader.ReadSingle(), m_Reader.ReadSingle());
		}
	}

	public void Color32(ref Color32 v)
	{
		if (WriteMode)
		{
			int value = v.r + (v.g << 8) + (v.b << 16) + (v.a << 24);
			m_Writer.Write(value);
		}
		else
		{
			int num = m_Reader.ReadInt32();
			v = new Color32((byte)((uint)num & 0xFFu), (byte)((uint)(num >> 8) & 0xFFu), (byte)((uint)(num >> 16) & 0xFFu), (byte)((uint)(num >> 24) & 0xFFu));
		}
	}

	public void ColorArray(ref Color[] cs)
	{
		if (WriteMode)
		{
			m_Writer.Write(cs.Length);
			for (int i = 0; i < cs.Length; i++)
			{
				Color(ref cs[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		cs = new Color[num];
		for (int j = 0; j < cs.Length; j++)
		{
			Color(ref cs[j]);
		}
	}

	public void Color32Array(ref Color32[] cs)
	{
		if (WriteMode)
		{
			m_Writer.Write(cs.Length);
			for (int i = 0; i < cs.Length; i++)
			{
				Color32(ref cs[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		cs = new Color32[num];
		for (int j = 0; j < cs.Length; j++)
		{
			Color32(ref cs[j]);
		}
	}

	public void Color32List(ref List<Color32> cs)
	{
		if (WriteMode)
		{
			m_Writer.Write(cs.Count);
			for (int i = 0; i < cs.Count; i++)
			{
				Color32 v = cs[i];
				Color32(ref v);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		cs = new List<Color32>();
		for (int j = 0; j < num; j++)
		{
			Color32 v2 = default(Color32);
			Color32(ref v2);
			cs.Add(v2);
		}
	}

	public void VectorIntList(ref List<Vector2Int> vs)
	{
		if (WriteMode)
		{
			m_Writer.Write(vs.Count);
			for (int i = 0; i < vs.Count; i++)
			{
				Vector2Int v = vs[i];
				VectorInt(ref v);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		vs = new List<Vector2Int>();
		for (int j = 0; j < num; j++)
		{
			Vector2Int v2 = default(Vector2Int);
			VectorInt(ref v2);
			vs.Add(v2);
		}
	}

	public void AnimationCurve(ref AnimationCurve cv)
	{
		if (WriteMode)
		{
			Keyframe[] keys = cv.keys;
			m_Writer.Write(keys.Length);
			Keyframe[] array = keys;
			for (int i = 0; i < array.Length; i++)
			{
				Keyframe keyframe = array[i];
				m_Writer.Write(keyframe.time);
				m_Writer.Write(keyframe.value);
				m_Writer.Write((byte)keyframe.weightedMode);
				m_Writer.Write(keyframe.inTangent);
				m_Writer.Write(keyframe.inWeight);
				m_Writer.Write(keyframe.outTangent);
				m_Writer.Write(keyframe.outWeight);
			}
		}
		else
		{
			Keyframe[] array2 = new Keyframe[m_Reader.ReadInt32()];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = new Keyframe
				{
					time = m_Reader.ReadSingle(),
					value = m_Reader.ReadSingle(),
					weightedMode = (WeightedMode)m_Reader.ReadByte(),
					inTangent = m_Reader.ReadSingle(),
					inWeight = m_Reader.ReadSingle(),
					outTangent = m_Reader.ReadSingle(),
					outWeight = m_Reader.ReadSingle()
				};
			}
			cv = new AnimationCurve(array2);
		}
	}

	public void Gradient(ref Gradient cv)
	{
		if (WriteMode)
		{
			GradientColorKey[] colorKeys = cv.colorKeys;
			m_Writer.Write(colorKeys.Length);
			GradientColorKey[] array = colorKeys;
			for (int i = 0; i < array.Length; i++)
			{
				GradientColorKey gradientColorKey = array[i];
				m_Writer.Write(gradientColorKey.time);
				m_Writer.Write(gradientColorKey.color.r);
				m_Writer.Write(gradientColorKey.color.g);
				m_Writer.Write(gradientColorKey.color.b);
			}
			GradientAlphaKey[] alphaKeys = cv.alphaKeys;
			m_Writer.Write(alphaKeys.Length);
			GradientAlphaKey[] array2 = alphaKeys;
			for (int i = 0; i < array2.Length; i++)
			{
				GradientAlphaKey gradientAlphaKey = array2[i];
				m_Writer.Write(gradientAlphaKey.time);
				m_Writer.Write(gradientAlphaKey.alpha);
			}
			m_Writer.Write((byte)cv.mode);
		}
		else
		{
			GradientColorKey[] array3 = new GradientColorKey[m_Reader.ReadInt32()];
			for (int j = 0; j < array3.Length; j++)
			{
				array3[j] = new GradientColorKey
				{
					time = m_Reader.ReadSingle(),
					color = new Color(m_Reader.ReadSingle(), m_Reader.ReadSingle(), m_Reader.ReadSingle())
				};
			}
			GradientAlphaKey[] array4 = new GradientAlphaKey[m_Reader.ReadInt32()];
			for (int k = 0; k < array4.Length; k++)
			{
				array4[k] = new GradientAlphaKey
				{
					time = m_Reader.ReadSingle(),
					alpha = m_Reader.ReadSingle()
				};
			}
			cv = new Gradient
			{
				colorKeys = array3,
				alphaKeys = array4,
				mode = (GradientMode)m_Reader.ReadByte()
			};
		}
	}

	public void ColorBlock(ref ColorBlock cb)
	{
		if (WriteMode)
		{
			Color v = cb.normalColor;
			Color(ref v);
			v = cb.pressedColor;
			Color(ref v);
			v = cb.highlightedColor;
			Color(ref v);
			v = cb.disabledColor;
			Color(ref v);
			m_Writer.Write(cb.colorMultiplier);
			m_Writer.Write(cb.fadeDuration);
		}
		else
		{
			cb = default(ColorBlock);
			Color v2 = default(Color);
			Color(ref v2);
			cb.normalColor = v2;
			Color(ref v2);
			cb.pressedColor = v2;
			Color(ref v2);
			cb.highlightedColor = v2;
			Color(ref v2);
			cb.disabledColor = v2;
			cb.colorMultiplier = m_Reader.ReadSingle();
			cb.fadeDuration = m_Reader.ReadSingle();
		}
	}

	public void UnityObject<T>(ref T obj) where T : UnityEngine.Object
	{
		if (WriteMode)
		{
			if (!obj)
			{
				m_Writer.Write(-1);
				return;
			}
			int value = m_ReferencedAssets.IndexOf(obj);
			_ = 0;
			m_Writer.Write(value);
			return;
		}
		int num = m_Reader.ReadInt32();
		if (num < 0)
		{
			obj = null;
			return;
		}
		UnityEngine.Object @object = m_ReferencedAssets.Get(num);
		if ((bool)@object && !(@object is T))
		{
			UberDebug.LogError($"Expected asset type {typeof(T).Name} at id {num} in the asset list");
		}
		obj = (T)@object;
	}

	public void UnityObjectArray<T>(ref T[] objs) where T : UnityEngine.Object
	{
		if (WriteMode)
		{
			m_Writer.Write(objs.Length);
			for (int i = 0; i < objs.Length; i++)
			{
				UnityObject(ref objs[i]);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		objs = new T[num];
		for (int j = 0; j < objs.Length; j++)
		{
			UnityObject(ref objs[j]);
		}
	}

	public void UnityObjectList<T>(ref List<T> objs) where T : UnityEngine.Object
	{
		if (WriteMode)
		{
			m_Writer.Write(objs.Count);
			for (int i = 0; i < objs.Count; i++)
			{
				T obj = objs[i];
				UnityObject(ref obj);
			}
			return;
		}
		int num = m_Reader.ReadInt32();
		objs = new List<T>();
		for (int j = 0; j < num; j++)
		{
			T obj2 = null;
			UnityObject(ref obj2);
			objs.Add(obj2);
		}
	}

	public void WriteType(Type t)
	{
		Guid valueOrDefault = (t?.GetCustomAttribute<TypeIdAttribute>()?.Guid).GetValueOrDefault();
		try
		{
			m_Writer.Write(valueOrDefault.ToByteArray());
		}
		catch (KeyNotFoundException)
		{
			throw new NotImplementedException("Type " + t.FullName + " is not found in type cache");
		}
	}

	public Type ReadType()
	{
		m_Reader.Read(m_GuidBuffer, 0, 16);
		Guid guid = new Guid(m_GuidBuffer);
		if (guid == default(Guid))
		{
			return null;
		}
		try
		{
			return ((GuidClassBinder)Json.Serializer.SerializationBinder).BindToType("", guid.ToString("N"));
		}
		catch (KeyNotFoundException)
		{
			throw new NotImplementedException($"TypeId {guid} is not found in type cache");
		}
	}
}
