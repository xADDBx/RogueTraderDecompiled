using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Utility.StatefulRandom;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public readonly struct SerializableRandState : IMemoryPackable<SerializableRandState>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SerializableRandStateFormatter : MemoryPackFormatter<SerializableRandState>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SerializableRandState value)
		{
			SerializableRandState.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SerializableRandState value)
		{
			SerializableRandState.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public readonly string Name;

	[JsonProperty]
	public readonly uint x;

	[JsonProperty]
	public readonly uint y;

	[JsonProperty]
	public readonly uint z;

	[JsonProperty]
	public readonly uint w;

	[MemoryPackIgnore]
	public RandState Value
	{
		get
		{
			RandState result = default(RandState);
			result.x = x;
			result.y = y;
			result.z = z;
			result.w = w;
			return result;
		}
	}

	[MemoryPackConstructor]
	private SerializableRandState(string name, uint x, uint y, uint z, uint w)
	{
		Name = name;
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public SerializableRandState(string name, RandState value)
	{
		Name = name;
		x = value.x;
		y = value.y;
		z = value.z;
		w = value.w;
	}

	static SerializableRandState()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SerializableRandState>())
		{
			MemoryPackFormatterProvider.Register(new SerializableRandStateFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SerializableRandState[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SerializableRandState>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SerializableRandState value)
	{
		writer.WriteObjectHeader(5);
		writer.WriteString(value.Name);
		writer.WriteUnmanaged(in value.x, in value.y, in value.z, in value.w);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SerializableRandState value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(SerializableRandState);
			return;
		}
		string name;
		uint value2;
		uint value3;
		uint value4;
		uint value5;
		if (memberCount == 5)
		{
			name = reader.ReadString();
			reader.ReadUnmanaged<uint, uint, uint, uint>(out value2, out value3, out value4, out value5);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SerializableRandState), 5, memberCount);
				return;
			}
			name = null;
			value2 = 0u;
			value3 = 0u;
			value4 = 0u;
			value5 = 0u;
			if (memberCount != 0)
			{
				name = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<uint>(out value2);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<uint>(out value3);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<uint>(out value4);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<uint>(out value5);
								_ = 5;
							}
						}
					}
				}
			}
		}
		value = new SerializableRandState(name, value2, value3, value4, value5);
	}
}
