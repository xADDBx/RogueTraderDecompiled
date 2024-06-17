using System;
using Kingmaker.Visual.CharacterSystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.ResourceLinks;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class EquipmentEntityLink : WeakResourceLink<EquipmentEntity>, IMemoryPackable<EquipmentEntityLink>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EquipmentEntityLinkFormatter : MemoryPackFormatter<EquipmentEntityLink>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EquipmentEntityLink value)
		{
			EquipmentEntityLink.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EquipmentEntityLink value)
		{
			EquipmentEntityLink.Deserialize(ref reader, ref value);
		}
	}

	static EquipmentEntityLink()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EquipmentEntityLink>())
		{
			MemoryPackFormatterProvider.Register(new EquipmentEntityLinkFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EquipmentEntityLink[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EquipmentEntityLink>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EquipmentEntityLink? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.AssetId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EquipmentEntityLink? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string assetId;
		if (memberCount == 1)
		{
			if (!(value == null))
			{
				assetId = value.AssetId;
				assetId = reader.ReadString();
				goto IL_007a;
			}
			assetId = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EquipmentEntityLink), 1, memberCount);
				return;
			}
			assetId = ((!(value == null)) ? value.AssetId : null);
			if (memberCount != 0)
			{
				assetId = reader.ReadString();
				_ = 1;
			}
			if (!(value == null))
			{
				goto IL_007a;
			}
		}
		value = new EquipmentEntityLink
		{
			AssetId = assetId
		};
		return;
		IL_007a:
		value.AssetId = assetId;
	}
}
