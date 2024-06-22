using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public class PortraitSaveMetaData : IMemoryPackable<PortraitSaveMetaData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PortraitSaveMetaDataFormatter : MemoryPackFormatter<PortraitSaveMetaData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PortraitSaveMetaData value)
		{
			PortraitSaveMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PortraitSaveMetaData value)
		{
			PortraitSaveMetaData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "g")]
	public Guid guid;

	[JsonProperty(PropertyName = "i")]
	public string originId;

	[JsonProperty(PropertyName = "l")]
	public int[] imagesFileLength;

	static PortraitSaveMetaData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitSaveMetaData>())
		{
			MemoryPackFormatterProvider.Register(new PortraitSaveMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PortraitSaveMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PortraitSaveMetaData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<int[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PortraitSaveMetaData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(3, in value.guid);
		writer.WriteString(value.originId);
		writer.WriteUnmanagedArray(value.imagesFileLength);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PortraitSaveMetaData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Guid value2;
		int[] value3;
		string text;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.guid;
				text = value.originId;
				value3 = value.imagesFileLength;
				reader.ReadUnmanaged<Guid>(out value2);
				text = reader.ReadString();
				reader.ReadUnmanagedArray(ref value3);
				goto IL_00cc;
			}
			reader.ReadUnmanaged<Guid>(out value2);
			text = reader.ReadString();
			value3 = reader.ReadUnmanagedArray<int>();
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PortraitSaveMetaData), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(Guid);
				text = null;
				value3 = null;
			}
			else
			{
				value2 = value.guid;
				text = value.originId;
				value3 = value.imagesFileLength;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Guid>(out value2);
				if (memberCount != 1)
				{
					text = reader.ReadString();
					if (memberCount != 2)
					{
						reader.ReadUnmanagedArray(ref value3);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00cc;
			}
		}
		value = new PortraitSaveMetaData
		{
			guid = value2,
			originId = text,
			imagesFileLength = value3
		};
		return;
		IL_00cc:
		value.guid = value2;
		value.originId = text;
		value.imagesFileLength = value3;
	}
}
