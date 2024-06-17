using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.Networking;

[MemoryPackable(GenerateType.Object)]
public class ModData : IEquatable<ModData>, IMemoryPackable<ModData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ModDataFormatter : MemoryPackFormatter<ModData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ModData value)
		{
			ModData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ModData value)
		{
			ModData.Deserialize(ref reader, ref value);
		}
	}

	public string Id;

	public string Version;

	public override string ToString()
	{
		return Id + "-" + Version;
	}

	public bool Equals(ModData other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Id == other.Id)
		{
			return Version == other.Version;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((ModData)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, Version);
	}

	static ModData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ModData>())
		{
			MemoryPackFormatterProvider.Register(new ModDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ModData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ModData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ModData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.Id);
		writer.WriteString(value.Version);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ModData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string id;
		string version;
		if (memberCount == 2)
		{
			if (value != null)
			{
				id = value.Id;
				version = value.Version;
				id = reader.ReadString();
				version = reader.ReadString();
				goto IL_0093;
			}
			id = reader.ReadString();
			version = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ModData), 2, memberCount);
				return;
			}
			if (value == null)
			{
				id = null;
				version = null;
			}
			else
			{
				id = value.Id;
				version = value.Version;
			}
			if (memberCount != 0)
			{
				id = reader.ReadString();
				if (memberCount != 1)
				{
					version = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0093;
			}
		}
		value = new ModData
		{
			Id = id,
			Version = version
		};
		return;
		IL_0093:
		value.Id = id;
		value.Version = version;
	}
}
