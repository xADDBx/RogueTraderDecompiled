using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public sealed class CreateColonyGameCommand : GameCommandWithSynchronized, IMemoryPackable<CreateColonyGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CreateColonyGameCommandFormatter : MemoryPackFormatter<CreateColonyGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CreateColonyGameCommand value)
		{
			CreateColonyGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CreateColonyGameCommand value)
		{
			CreateColonyGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<PlanetEntity> m_Planet;

	[JsonProperty]
	[MemoryPackInclude]
	private List<BlueprintColonyEventReference> m_Events;

	[MemoryPackConstructor]
	private CreateColonyGameCommand()
	{
	}

	[JsonConstructor]
	public CreateColonyGameCommand([NotNull] PlanetEntity planet, List<BlueprintColonyEvent> events, bool isSynchronized = true)
	{
		m_Planet = planet;
		m_Events = events.Select((BlueprintColonyEvent ev) => ev.ToReference<BlueprintColonyEventReference>()).ToList();
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		PlanetEntity planet = m_Planet.Entity;
		if (planet == null)
		{
			return;
		}
		List<BlueprintColonyEventsRoot.ColonyEventToTimer> list = new List<BlueprintColonyEventsRoot.ColonyEventToTimer>();
		List<BlueprintColonyEventsRoot.ColonyEventToTimer> source = BlueprintWarhammerRoot.Instance.ColonyRoot.ColonyEvents.Get().Events.ToList();
		foreach (BlueprintColonyEvent ev in m_Events.Dereference())
		{
			BlueprintColonyEventsRoot.ColonyEventToTimer colonyEventToTimer = source.FirstOrDefault((BlueprintColonyEventsRoot.ColonyEventToTimer e) => e.ColonyEvent == ev);
			if (colonyEventToTimer != null)
			{
				list.Add(colonyEventToTimer);
			}
		}
		Colony colony = new Colony(m_Planet, list);
		planet.Colony = colony;
		Dictionary<BlueprintPlanet, BlueprintPlanetPrefab> planetChangedVisualPrefabs = Game.Instance.Player.StarSystemsState.PlanetChangedVisualPrefabs;
		if (planetChangedVisualPrefabs.ContainsKey(planet.Blueprint))
		{
			planetChangedVisualPrefabs[planet.Blueprint] = colony.Blueprint.PlanetVisual;
		}
		else
		{
			planetChangedVisualPrefabs.Add(planet.Blueprint, colony.Blueprint.PlanetVisual);
		}
		(Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity sso) => sso.Blueprint == planet.Blueprint) as PlanetEntity)?.View.SetNewVisual();
		PFLog.Default.Log("Planet " + planet.Blueprint.Name + " colonized");
		Game.Instance.ColonizationController.LastColonizedPlanet = planet;
		Game.Instance.ColonizationController.NeedToOpenExplorationScreen = true;
	}

	static CreateColonyGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CreateColonyGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CreateColonyGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CreateColonyGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CreateColonyGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<BlueprintColonyEventReference>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<BlueprintColonyEventReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CreateColonyGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Planet);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.m_Events));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CreateColonyGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<PlanetEntity> value2;
		List<BlueprintColonyEventReference> value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Planet;
				value3 = value.m_Events;
				reader.ReadPackable(ref value2);
				ListFormatter.DeserializePackable(ref reader, ref value3);
				goto IL_00a0;
			}
			value2 = reader.ReadPackable<EntityRef<PlanetEntity>>();
			value3 = ListFormatter.DeserializePackable<BlueprintColonyEventReference>(ref reader);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CreateColonyGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<PlanetEntity>);
				value3 = null;
			}
			else
			{
				value2 = value.m_Planet;
				value3 = value.m_Events;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					ListFormatter.DeserializePackable(ref reader, ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a0;
			}
		}
		value = new CreateColonyGameCommand
		{
			m_Planet = value2,
			m_Events = value3
		};
		return;
		IL_00a0:
		value.m_Planet = value2;
		value.m_Events = value3;
	}
}
