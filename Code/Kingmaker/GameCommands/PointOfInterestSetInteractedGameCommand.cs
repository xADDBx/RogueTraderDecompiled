using System.Linq;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class PointOfInterestSetInteractedGameCommand : GameCommand, IMemoryPackable<PointOfInterestSetInteractedGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PointOfInterestSetInteractedGameCommandFormatter : MemoryPackFormatter<PointOfInterestSetInteractedGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PointOfInterestSetInteractedGameCommand value)
		{
			PointOfInterestSetInteractedGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PointOfInterestSetInteractedGameCommand value)
		{
			PointOfInterestSetInteractedGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintPointOfInterest m_Poi;

	[MemoryPackConstructor]
	private PointOfInterestSetInteractedGameCommand()
	{
	}

	[JsonConstructor]
	public PointOfInterestSetInteractedGameCommand(BlueprintPointOfInterest poi)
	{
		m_Poi = poi;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity s) => s.PointOfInterests.FirstOrDefault((BasePointOfInterest p) => p.Blueprint == m_Poi) != null)?.PointOfInterests.FirstOrDefault((BasePointOfInterest p) => p.Blueprint == m_Poi)?.ChangeStatusToInteracted();
	}

	static PointOfInterestSetInteractedGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PointOfInterestSetInteractedGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PointOfInterestSetInteractedGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PointOfInterestSetInteractedGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PointOfInterestSetInteractedGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PointOfInterestSetInteractedGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteValue(in value.m_Poi);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PointOfInterestSetInteractedGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPointOfInterest value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Poi;
				reader.ReadValue(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadValue<BlueprintPointOfInterest>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PointOfInterestSetInteractedGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Poi : null);
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new PointOfInterestSetInteractedGameCommand
		{
			m_Poi = value2
		};
		return;
		IL_006a:
		value.m_Poi = value2;
	}
}
