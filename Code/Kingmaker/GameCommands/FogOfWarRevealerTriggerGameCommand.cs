using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class FogOfWarRevealerTriggerGameCommand : GameCommand, IMemoryPackable<FogOfWarRevealerTriggerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class FogOfWarRevealerTriggerGameCommandFormatter : MemoryPackFormatter<FogOfWarRevealerTriggerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref FogOfWarRevealerTriggerGameCommand value)
		{
			FogOfWarRevealerTriggerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FogOfWarRevealerTriggerGameCommand value)
		{
			FogOfWarRevealerTriggerGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly string Id;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public FogOfWarRevealerTriggerGameCommand(string id)
	{
		Id = id;
	}

	protected override void ExecuteInternal()
	{
		if (!FogOfWarRevealerTrigger.AllTriggers.TryGetValue(Id, out var value))
		{
			PFLog.GameCommands.Error("FogOfWarRevealerTrigger #" + Id + " was not found!");
		}
		else
		{
			value.Reveal();
		}
	}

	static FogOfWarRevealerTriggerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FogOfWarRevealerTriggerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new FogOfWarRevealerTriggerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FogOfWarRevealerTriggerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<FogOfWarRevealerTriggerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref FogOfWarRevealerTriggerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.Id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref FogOfWarRevealerTriggerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string id;
		if (memberCount == 1)
		{
			if (value == null)
			{
				id = reader.ReadString();
			}
			else
			{
				id = value.Id;
				id = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FogOfWarRevealerTriggerGameCommand), 1, memberCount);
				return;
			}
			id = ((value != null) ? value.Id : null);
			if (memberCount != 0)
			{
				id = reader.ReadString();
				_ = 1;
			}
			_ = value;
		}
		value = new FogOfWarRevealerTriggerGameCommand(id);
	}
}
