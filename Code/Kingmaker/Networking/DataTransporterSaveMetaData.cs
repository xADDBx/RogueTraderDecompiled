using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public struct DataTransporterSaveMetaData : IMemoryPackable<DataTransporterSaveMetaData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DataTransporterSaveMetaDataFormatter : MemoryPackFormatter<DataTransporterSaveMetaData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DataTransporterSaveMetaData value)
		{
			DataTransporterSaveMetaData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DataTransporterSaveMetaData value)
		{
			DataTransporterSaveMetaData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "u")]
	public int SenderUniqueNumber;

	[JsonProperty(PropertyName = "l")]
	public int Length;

	static DataTransporterSaveMetaData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DataTransporterSaveMetaData>())
		{
			MemoryPackFormatterProvider.Register(new DataTransporterSaveMetaDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DataTransporterSaveMetaData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DataTransporterSaveMetaData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DataTransporterSaveMetaData value)
	{
		writer.WriteUnmanaged(in value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DataTransporterSaveMetaData value)
	{
		reader.ReadUnmanaged<DataTransporterSaveMetaData>(out value);
	}
}
