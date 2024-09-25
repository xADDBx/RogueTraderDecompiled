using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public struct AvatarMetaData : IMemoryPackable<AvatarMetaData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AvatarMetaDataFormatter : MemoryPackFormatter<AvatarMetaData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AvatarMetaData value)
		{
			AvatarMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AvatarMetaData value)
		{
			AvatarMetaData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "u")]
	public int SenderUniqueNumber;

	[JsonProperty(PropertyName = "l")]
	public int SaveLength;

	[JsonProperty(PropertyName = "w")]
	public int AvatarWidth;

	static AvatarMetaData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AvatarMetaData>())
		{
			MemoryPackFormatterProvider.Register(new AvatarMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AvatarMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AvatarMetaData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AvatarMetaData value)
	{
		writer.WriteUnmanaged(in value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AvatarMetaData value)
	{
		reader.ReadUnmanaged<AvatarMetaData>(out value);
	}
}
