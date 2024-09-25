using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class TestLoadingProcessCommandsLogicGameCommand : GameCommand, IMemoryPackable<TestLoadingProcessCommandsLogicGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class TestLoadingProcessCommandsLogicGameCommandFormatter : MemoryPackFormatter<TestLoadingProcessCommandsLogicGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TestLoadingProcessCommandsLogicGameCommand value)
		{
			TestLoadingProcessCommandsLogicGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TestLoadingProcessCommandsLogicGameCommand value)
		{
			TestLoadingProcessCommandsLogicGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Counter;

	[JsonProperty]
	[MemoryPackInclude]
	private NetPlayerSerializable m_Player;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	protected TestLoadingProcessCommandsLogicGameCommand()
	{
	}

	[MemoryPackConstructor]
	public TestLoadingProcessCommandsLogicGameCommand(int m_counter, NetPlayerSerializable m_player)
	{
		m_Counter = m_counter;
		m_Player = m_player;
	}

	public TestLoadingProcessCommandsLogicGameCommand(int counter, NetPlayer player)
		: this(counter, (NetPlayerSerializable)player)
	{
		PFLog.Net.Log($"TestLoadingProcessCommandsLogicGameCommand new {m_Counter} {((NetPlayer)m_Player).ToString()}");
	}

	protected override void ExecuteInternal()
	{
		PFLog.Net.Log($"TestLoadingProcessCommandsLogicGameCommand exe {m_Counter} {((NetPlayer)m_Player).ToString()}");
		if (m_Counter != 0 && NetworkingManager.LocalNetPlayer.Equals((NetPlayer)m_Player))
		{
			Game.Instance.GameCommandQueue.TestLoadingProcessCommandsLogicGameCommand(m_Counter - 1, (NetPlayer)m_Player);
		}
	}

	static TestLoadingProcessCommandsLogicGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TestLoadingProcessCommandsLogicGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TestLoadingProcessCommandsLogicGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TestLoadingProcessCommandsLogicGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TestLoadingProcessCommandsLogicGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TestLoadingProcessCommandsLogicGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_Counter, in value.m_Player);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TestLoadingProcessCommandsLogicGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		NetPlayerSerializable value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, NetPlayerSerializable>(out value2, out value3);
			}
			else
			{
				value2 = value.m_Counter;
				value3 = value.m_Player;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<NetPlayerSerializable>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TestLoadingProcessCommandsLogicGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = default(NetPlayerSerializable);
			}
			else
			{
				value2 = value.m_Counter;
				value3 = value.m_Player;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<NetPlayerSerializable>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new TestLoadingProcessCommandsLogicGameCommand(value2, value3);
	}
}
