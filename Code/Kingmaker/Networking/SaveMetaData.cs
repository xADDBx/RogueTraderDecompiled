using Kingmaker.Networking.Settings;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public class SaveMetaData : IMemoryPackable<SaveMetaData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SaveMetaDataFormatter : MemoryPackFormatter<SaveMetaData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveMetaData value)
		{
			SaveMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveMetaData value)
		{
			SaveMetaData.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackIgnore]
	public static int MaxPacketSize;

	[JsonProperty(PropertyName = "l")]
	public int length;

	[JsonProperty(PropertyName = "n")]
	public string saveName;

	[JsonProperty(PropertyName = "i")]
	public string saveId;

	[JsonProperty(PropertyName = "r")]
	public uint randomNoise;

	[JsonProperty(PropertyName = "a")]
	public PhotonActorNumber[] actorNumbersAtStart;

	[JsonProperty(PropertyName = "d")]
	public string[] dlcs;

	[JsonProperty(PropertyName = "s")]
	public BaseSettingNetData[] settings;

	static SaveMetaData()
	{
		MaxPacketSize = 65536;
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaData>())
		{
			MemoryPackFormatterProvider.Register(new SaveMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveMetaData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PhotonActorNumber[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PhotonActorNumber>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<string[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<string>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BaseSettingNetData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BaseSettingNetData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveMetaData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(7, in value.length);
		writer.WriteString(value.saveName);
		writer.WriteString(value.saveId);
		writer.WriteUnmanaged(in value.randomNoise);
		writer.WriteUnmanagedArray(value.actorNumbersAtStart);
		writer.WriteArray(value.dlcs);
		writer.WriteArray(value.settings);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveMetaData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		uint value3;
		PhotonActorNumber[] value4;
		string[] value5;
		BaseSettingNetData[] value6;
		string text;
		string text2;
		if (memberCount == 7)
		{
			if (value != null)
			{
				value2 = value.length;
				text = value.saveName;
				text2 = value.saveId;
				value3 = value.randomNoise;
				value4 = value.actorNumbersAtStart;
				value5 = value.dlcs;
				value6 = value.settings;
				reader.ReadUnmanaged<int>(out value2);
				text = reader.ReadString();
				text2 = reader.ReadString();
				reader.ReadUnmanaged<uint>(out value3);
				reader.ReadUnmanagedArray(ref value4);
				reader.ReadArray(ref value5);
				reader.ReadArray(ref value6);
				goto IL_018e;
			}
			reader.ReadUnmanaged<int>(out value2);
			text = reader.ReadString();
			text2 = reader.ReadString();
			reader.ReadUnmanaged<uint>(out value3);
			value4 = reader.ReadUnmanagedArray<PhotonActorNumber>();
			value5 = reader.ReadArray<string>();
			value6 = reader.ReadArray<BaseSettingNetData>();
		}
		else
		{
			if (memberCount > 7)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveMetaData), 7, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				text = null;
				text2 = null;
				value3 = 0u;
				value4 = null;
				value5 = null;
				value6 = null;
			}
			else
			{
				value2 = value.length;
				text = value.saveName;
				text2 = value.saveId;
				value3 = value.randomNoise;
				value4 = value.actorNumbersAtStart;
				value5 = value.dlcs;
				value6 = value.settings;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					text = reader.ReadString();
					if (memberCount != 2)
					{
						text2 = reader.ReadString();
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<uint>(out value3);
							if (memberCount != 4)
							{
								reader.ReadUnmanagedArray(ref value4);
								if (memberCount != 5)
								{
									reader.ReadArray(ref value5);
									if (memberCount != 6)
									{
										reader.ReadArray(ref value6);
										_ = 7;
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_018e;
			}
		}
		value = new SaveMetaData
		{
			length = value2,
			saveName = text,
			saveId = text2,
			randomNoise = value3,
			actorNumbersAtStart = value4,
			dlcs = value5,
			settings = value6
		};
		return;
		IL_018e:
		value.length = value2;
		value.saveName = text;
		value.saveId = text2;
		value.randomNoise = value3;
		value.actorNumbersAtStart = value4;
		value.dlcs = value5;
		value.settings = value6;
	}
}
