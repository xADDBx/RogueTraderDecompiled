using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine.Experimental.Rendering;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public struct ImageMetaData : IMemoryPackable<ImageMetaData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ImageMetaDataFormatter : MemoryPackFormatter<ImageMetaData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ImageMetaData value)
		{
			ImageMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ImageMetaData value)
		{
			ImageMetaData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "u")]
	public int SenderUniqueNumber;

	[JsonProperty(PropertyName = "l")]
	public int SaveLength;

	[JsonProperty(PropertyName = "w")]
	public int Width;

	[JsonProperty(PropertyName = "h")]
	public int Height;

	[JsonProperty(PropertyName = "f")]
	public GraphicsFormat Format;

	static ImageMetaData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ImageMetaData>())
		{
			MemoryPackFormatterProvider.Register(new ImageMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ImageMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ImageMetaData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GraphicsFormat>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<GraphicsFormat>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ImageMetaData value)
	{
		writer.WriteUnmanaged(in value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ImageMetaData value)
	{
		reader.ReadUnmanaged<ImageMetaData>(out value);
	}
}
