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
		writer.WriteObjectHeader(4);
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
		UnitReference value2;
		sbyte value3;
		sbyte value4;
		UnitReference[] value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.unit;
				value3 = value.moveDirectionX;
				value4 = value.moveDirectionY;
				value5 = value.selectedUnits;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<sbyte>(out value3);
				reader.ReadUnmanaged<sbyte>(out value4);
				reader.ReadPackableArray(ref value5);
				goto IL_00fd;
			}
			value2 = reader.ReadPackable<UnitReference>();
			reader.ReadUnmanaged<sbyte, sbyte>(out value3, out value4);
			value5 = reader.ReadPackableArray<UnitReference>();
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LeftStickData), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(UnitReference);
				value3 = 0;
				value4 = 0;
				value5 = null;
			}
			else
			{
				value2 = value.unit;
				value3 = value.moveDirectionX;
				value4 = value.moveDirectionY;
				value5 = value.selectedUnits;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<sbyte>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<sbyte>(out value4);
						if (memberCount != 3)
						{
							reader.ReadPackableArray(ref value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00fd;
			}
		}
		value = new LeftStickData
		{
			unit = value2,
			moveDirectionX = value3,
			moveDirectionY = value4,
			selectedUnits = value5
		};
		return;
		IL_00fd:
		value.unit = value2;
		value.moveDirectionX = value3;
		value.moveDirectionY = value4;
		value.selectedUnits = value5;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
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
