using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public class SaveMetaAcknowledgeData : IMemoryPackable<SaveMetaAcknowledgeData>, IMemoryPackFormatterRegister
{
	public struct GuidData
	{
		public Guid Guid;

		public bool AlreadyHave;
	}

	[Preserve]
	private sealed class SaveMetaAcknowledgeDataFormatter : MemoryPackFormatter<SaveMetaAcknowledgeData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveMetaAcknowledgeData value)
		{
			SaveMetaAcknowledgeData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveMetaAcknowledgeData value)
		{
			SaveMetaAcknowledgeData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "g")]
	public GuidData[] PortraitsGuid;

	static SaveMetaAcknowledgeData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaAcknowledgeData>())
		{
			MemoryPackFormatterProvider.Register(new SaveMetaAcknowledgeDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaAcknowledgeData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveMetaAcknowledgeData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GuidData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<GuidData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveMetaAcknowledgeData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteUnmanagedArray(value.PortraitsGuid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveMetaAcknowledgeData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		GuidData[] value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.PortraitsGuid;
				reader.ReadUnmanagedArray(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadUnmanagedArray<GuidData>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveMetaAcknowledgeData), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.PortraitsGuid : null);
			if (memberCount != 0)
			{
				reader.ReadUnmanagedArray(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new SaveMetaAcknowledgeData
		{
			PortraitsGuid = value2
		};
		return;
		IL_006a:
		value.PortraitsGuid = value2;
	}
}
