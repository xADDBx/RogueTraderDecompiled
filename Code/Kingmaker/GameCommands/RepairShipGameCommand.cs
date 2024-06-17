using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class RepairShipGameCommand : GameCommand, IMemoryPackable<RepairShipGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RepairShipGameCommandFormatter : MemoryPackFormatter<RepairShipGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RepairShipGameCommand value)
		{
			RepairShipGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RepairShipGameCommand value)
		{
			RepairShipGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private int m_RestoreHealth;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RepairShipGameCommand()
	{
	}

	[JsonConstructor]
	public RepairShipGameCommand(int restoreHealth)
	{
		m_RestoreHealth = restoreHealth;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.Scrap.RepairShipInternal(m_RestoreHealth);
	}

	static RepairShipGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RepairShipGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RepairShipGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RepairShipGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RepairShipGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RepairShipGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_RestoreHealth);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RepairShipGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_RestoreHealth;
				reader.ReadUnmanaged<int>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<int>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RepairShipGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_RestoreHealth : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new RepairShipGameCommand
		{
			m_RestoreHealth = value2
		};
		return;
		IL_006b:
		value.m_RestoreHealth = value2;
	}
}
