using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Internal;

namespace Kingmaker.Networking.Settings;

[Preserve]
internal class BaseSettingNetDataFormatter : MemoryPackFormatter<BaseSettingNetData>
{
	private static readonly Dictionary<Type, ushort> __typeToTag = new Dictionary<Type, ushort>(4)
	{
		{
			typeof(BoolSettingNetData),
			0
		},
		{
			typeof(IntSettingNetData),
			1
		},
		{
			typeof(FloatSettingNetData),
			2
		},
		{
			typeof(EnumSettingNetData),
			3
		}
	};

	[Preserve]
	public override void Serialize(ref MemoryPackWriter writer, ref BaseSettingNetData? value)
	{
		ushort value2;
		if (value == null)
		{
			writer.WriteNullUnionHeader();
		}
		else if (__typeToTag.TryGetValue(value.GetType(), out value2))
		{
			writer.WriteUnionHeader(value2);
			switch (value2)
			{
			case 0:
				writer.WritePackable(in Unsafe.As<BaseSettingNetData, BoolSettingNetData>(ref value));
				break;
			case 1:
				writer.WritePackable(in Unsafe.As<BaseSettingNetData, IntSettingNetData>(ref value));
				break;
			case 2:
				writer.WritePackable(in Unsafe.As<BaseSettingNetData, FloatSettingNetData>(ref value));
				break;
			case 3:
				writer.WritePackable(in Unsafe.As<BaseSettingNetData, EnumSettingNetData>(ref value));
				break;
			}
		}
		else
		{
			MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof(BaseSettingNetData));
		}
	}

	[Preserve]
	public override void Deserialize(ref MemoryPackReader reader, ref BaseSettingNetData? value)
	{
		if (!reader.TryReadUnionHeader(out var tag))
		{
			value = null;
			return;
		}
		switch (tag)
		{
		case 0:
			if (value is BoolSettingNetData)
			{
				reader.ReadPackable(ref Unsafe.As<BaseSettingNetData, BoolSettingNetData>(ref value));
			}
			else
			{
				value = reader.ReadPackable<BoolSettingNetData>();
			}
			break;
		case 1:
			if (value is IntSettingNetData)
			{
				reader.ReadPackable(ref Unsafe.As<BaseSettingNetData, IntSettingNetData>(ref value));
			}
			else
			{
				value = reader.ReadPackable<IntSettingNetData>();
			}
			break;
		case 2:
			if (value is FloatSettingNetData)
			{
				reader.ReadPackable(ref Unsafe.As<BaseSettingNetData, FloatSettingNetData>(ref value));
			}
			else
			{
				value = reader.ReadPackable<FloatSettingNetData>();
			}
			break;
		case 3:
			if (value is EnumSettingNetData)
			{
				reader.ReadPackable(ref Unsafe.As<BaseSettingNetData, EnumSettingNetData>(ref value));
			}
			else
			{
				value = reader.ReadPackable<EnumSettingNetData>();
			}
			break;
		default:
			MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(BaseSettingNetData));
			break;
		}
	}
}
