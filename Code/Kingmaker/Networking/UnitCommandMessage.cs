using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Commands.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[JsonObject]
[MemoryPackable(GenerateType.Object)]
public struct UnitCommandMessage : IMemoryPackable<UnitCommandMessage>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitCommandMessageFormatter : MemoryPackFormatter<UnitCommandMessage>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitCommandMessage value)
		{
			UnitCommandMessage.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitCommandMessage value)
		{
			UnitCommandMessage.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "i")]
	public int tickIndex;

	[JsonProperty(PropertyName = "g")]
	public List<GameCommand> gameCommandList;

	[JsonProperty(PropertyName = "c")]
	public List<UnitCommandParams> unitCommandList;

	[JsonProperty(PropertyName = "d")]
	public List<SynchronizedData> synchronizedDataList;

	[MemoryPackConstructor]
	public UnitCommandMessage(int tickIndex, List<GameCommand> gameCommandList, List<UnitCommandParams> unitCommandList, List<SynchronizedData> synchronizedDataList)
	{
		this.tickIndex = tickIndex;
		this.gameCommandList = ((0 < gameCommandList?.Count) ? gameCommandList : null);
		this.unitCommandList = ((0 < unitCommandList?.Count) ? unitCommandList : null);
		this.synchronizedDataList = ((0 < synchronizedDataList?.Count) ? synchronizedDataList : null);
	}

	public void AfterDeserialization()
	{
		if (gameCommandList != null)
		{
			foreach (GameCommand gameCommand in gameCommandList)
			{
				gameCommand.AfterDeserialization();
			}
		}
		if (unitCommandList == null)
		{
			return;
		}
		foreach (UnitCommandParams unitCommand in unitCommandList)
		{
			unitCommand.AfterDeserialization();
		}
	}

	static UnitCommandMessage()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitCommandMessage>())
		{
			MemoryPackFormatterProvider.Register(new UnitCommandMessageFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitCommandMessage[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitCommandMessage>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<GameCommand>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<GameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<UnitCommandParams>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<UnitCommandParams>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<SynchronizedData>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<SynchronizedData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnitCommandMessage value)
	{
		writer.WriteUnmanagedWithObjectHeader(4, in value.tickIndex);
		writer.WriteValue(in value.gameCommandList);
		writer.WriteValue(in value.unitCommandList);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.synchronizedDataList));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitCommandMessage value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(UnitCommandMessage);
			return;
		}
		int value2;
		List<GameCommand> value3;
		List<UnitCommandParams> value4;
		List<SynchronizedData> value5;
		if (memberCount == 4)
		{
			reader.ReadUnmanaged<int>(out value2);
			value3 = reader.ReadValue<List<GameCommand>>();
			value4 = reader.ReadValue<List<UnitCommandParams>>();
			value5 = ListFormatter.DeserializePackable<SynchronizedData>(ref reader);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitCommandMessage), 4, memberCount);
				return;
			}
			value2 = 0;
			value3 = null;
			value4 = null;
			value5 = null;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					if (memberCount != 2)
					{
						reader.ReadValue(ref value4);
						if (memberCount != 3)
						{
							ListFormatter.DeserializePackable(ref reader, ref value5);
							_ = 4;
						}
					}
				}
			}
		}
		value = new UnitCommandMessage(value2, value3, value4, value5);
	}
}
