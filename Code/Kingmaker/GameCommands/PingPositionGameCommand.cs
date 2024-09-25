using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PingPositionGameCommand : GameCommand, IMemoryPackable<PingPositionGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PingPositionGameCommandFormatter : MemoryPackFormatter<PingPositionGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PingPositionGameCommand value)
		{
			PingPositionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingPositionGameCommand value)
		{
			PingPositionGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private Vector3 m_Position;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingPositionGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingPositionGameCommand(Vector3 m_position)
	{
		m_Position = m_position;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingPositionLocally(playerOrEmpty, m_Position);
	}

	static PingPositionGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingPositionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingPositionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingPositionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingPositionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PingPositionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Position);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingPositionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Vector3 value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
			}
			else
			{
				value2 = value.m_Position;
				reader.ReadUnmanaged<Vector3>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingPositionGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Position : default(Vector3));
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PingPositionGameCommand(value2);
	}
}
