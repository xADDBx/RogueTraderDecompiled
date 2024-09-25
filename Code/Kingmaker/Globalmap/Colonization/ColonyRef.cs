using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Globalmap.Colonization;

[MemoryPackable(GenerateType.Object)]
public readonly struct ColonyRef : IMemoryPackable<ColonyRef>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ColonyRefFormatter : MemoryPackFormatter<ColonyRef>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ColonyRef value)
		{
			ColonyRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ColonyRef value)
		{
			ColonyRef.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly string BlueprintId;

	[MemoryPackConstructor]
	public ColonyRef(string blueprintId)
	{
		BlueprintId = blueprintId;
	}

	public ColonyRef(Colony colony)
	{
		BlueprintId = colony.Blueprint.AssetGuid;
	}

	public bool TryGet(out Colony colony)
	{
		return Game.Instance.Player.ColoniesState.TryGetColonyByGuid(BlueprintId, out colony);
	}

	public static explicit operator Colony(ColonyRef colonyRef)
	{
		if (!colonyRef.TryGet(out var colony))
		{
			return null;
		}
		return colony;
	}

	public static explicit operator ColonyRef(Colony colony)
	{
		return new ColonyRef(colony);
	}

	public override string ToString()
	{
		return "[ColonyRef:" + BlueprintId + "]";
	}

	static ColonyRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyRef>())
		{
			MemoryPackFormatterProvider.Register(new ColonyRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ColonyRef>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ColonyRef value)
	{
		writer.WriteObjectHeader(1);
		writer.WriteString(value.BlueprintId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ColonyRef value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(ColonyRef);
			return;
		}
		string blueprintId;
		if (memberCount == 1)
		{
			blueprintId = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ColonyRef), 1, memberCount);
				return;
			}
			blueprintId = null;
			if (memberCount != 0)
			{
				blueprintId = reader.ReadString();
				_ = 1;
			}
		}
		value = new ColonyRef(blueprintId);
	}
}
