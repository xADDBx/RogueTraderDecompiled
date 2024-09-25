using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public struct CustomPortraitMetaData : IMemoryPackable<CustomPortraitMetaData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CustomPortraitMetaDataFormatter : MemoryPackFormatter<CustomPortraitMetaData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CustomPortraitMetaData value)
		{
			CustomPortraitMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CustomPortraitMetaData value)
		{
			CustomPortraitMetaData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "u")]
	public int SenderUniqueNumber;

	[JsonProperty(PropertyName = "s")]
	public int LengthSmallPortrait;

	[JsonProperty(PropertyName = "h")]
	public int LengthHalfPortrait;

	[JsonProperty(PropertyName = "f")]
	public int LengthFullPortrait;

	[JsonProperty(PropertyName = "i")]
	public string CustomPortraitId;

	[JsonProperty(PropertyName = "g")]
	public Guid PortraitGuid;

	static CustomPortraitMetaData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CustomPortraitMetaData>())
		{
			MemoryPackFormatterProvider.Register(new CustomPortraitMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CustomPortraitMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CustomPortraitMetaData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CustomPortraitMetaData value)
	{
		writer.WriteUnmanagedWithObjectHeader(6, in value.SenderUniqueNumber, in value.LengthSmallPortrait, in value.LengthHalfPortrait, in value.LengthFullPortrait);
		writer.WriteString(value.CustomPortraitId);
		writer.WriteUnmanaged(in value.PortraitGuid);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CustomPortraitMetaData value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(CustomPortraitMetaData);
			return;
		}
		int value2;
		int value3;
		int value4;
		int value5;
		string customPortraitId;
		Guid value6;
		if (memberCount == 6)
		{
			reader.ReadUnmanaged<int, int, int, int>(out value2, out value3, out value4, out value5);
			customPortraitId = reader.ReadString();
			reader.ReadUnmanaged<Guid>(out value6);
		}
		else
		{
			if (memberCount > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CustomPortraitMetaData), 6, memberCount);
				return;
			}
			value2 = 0;
			value3 = 0;
			value4 = 0;
			value5 = 0;
			customPortraitId = null;
			value6 = default(Guid);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<int>(out value5);
							if (memberCount != 4)
							{
								customPortraitId = reader.ReadString();
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<Guid>(out value6);
									_ = 6;
								}
							}
						}
					}
				}
			}
		}
		value = new CustomPortraitMetaData
		{
			SenderUniqueNumber = value2,
			LengthSmallPortrait = value3,
			LengthHalfPortrait = value4,
			LengthFullPortrait = value5,
			CustomPortraitId = customPortraitId,
			PortraitGuid = value6
		};
	}
}
