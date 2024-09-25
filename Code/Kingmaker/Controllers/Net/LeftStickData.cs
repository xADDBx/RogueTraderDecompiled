using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public class LeftStickData : IMemoryPackable<LeftStickData>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class LeftStickDataFormatter : MemoryPackFormatter<LeftStickData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LeftStickData value)
		{
			LeftStickData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LeftStickData value)
		{
			LeftStickData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "v")]
	public byte version;

	[JsonProperty(PropertyName = "u")]
	public UnitReference unit;

	[JsonProperty(PropertyName = "x")]
	public sbyte moveDirectionX;

	[JsonProperty(PropertyName = "y")]
	public sbyte moveDirectionY;

	[JsonProperty(PropertyName = "s")]
	public UnitReference[] selectedUnits;

	static LeftStickData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LeftStickData>())
		{
			MemoryPackFormatterProvider.Register(new LeftStickDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LeftStickData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<LeftStickData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LeftStickData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(5, in value.version);
		writer.WritePackable(in value.unit);
		writer.WriteUnmanaged(in value.moveDirectionX, in value.moveDirectionY);
		writer.WritePackableArray(value.selectedUnits);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LeftStickData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		UnitReference value3;
		sbyte value4;
		sbyte value5;
		UnitReference[] value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.version;
				value3 = value.unit;
				value4 = value.moveDirectionX;
				value5 = value.moveDirectionY;
				value6 = value.selectedUnits;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<sbyte>(out value4);
				reader.ReadUnmanaged<sbyte>(out value5);
				reader.ReadPackableArray(ref value6);
				goto IL_0131;
			}
			reader.ReadUnmanaged<byte>(out value2);
			value3 = reader.ReadPackable<UnitReference>();
			reader.ReadUnmanaged<sbyte, sbyte>(out value4, out value5);
			value6 = reader.ReadPackableArray<UnitReference>();
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LeftStickData), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = default(UnitReference);
				value4 = 0;
				value5 = 0;
				value6 = null;
			}
			else
			{
				value2 = value.version;
				value3 = value.unit;
				value4 = value.moveDirectionX;
				value5 = value.moveDirectionY;
				value6 = value.selectedUnits;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<sbyte>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<sbyte>(out value5);
							if (memberCount != 4)
							{
								reader.ReadPackableArray(ref value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0131;
			}
		}
		value = new LeftStickData
		{
			version = value2,
			unit = value3,
			moveDirectionX = value4,
			moveDirectionY = value5,
			selectedUnits = value6
		};
		return;
		IL_0131:
		value.version = value2;
		value.unit = value3;
		value.moveDirectionX = value4;
		value.moveDirectionY = value5;
		value.selectedUnits = value6;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref version);
		UnitReference obj = unit;
		Hash128 val = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val);
		result.Append(ref moveDirectionX);
		result.Append(ref moveDirectionY);
		UnitReference[] array = selectedUnits;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				UnitReference obj2 = array[i];
				Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj2);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
