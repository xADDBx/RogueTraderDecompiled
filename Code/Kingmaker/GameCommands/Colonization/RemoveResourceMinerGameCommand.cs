using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class RemoveResourceMinerGameCommand : GameCommand, IMemoryPackable<RemoveResourceMinerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RemoveResourceMinerGameCommandFormatter : MemoryPackFormatter<RemoveResourceMinerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RemoveResourceMinerGameCommand value)
		{
			RemoveResourceMinerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RemoveResourceMinerGameCommand value)
		{
			RemoveResourceMinerGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public BlueprintStarSystemObjectReference BlueprintSso;

	[JsonProperty]
	[MemoryPackInclude]
	public BlueprintResourceReference BlueprintResource;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RemoveResourceMinerGameCommand()
	{
	}

	[JsonConstructor]
	public RemoveResourceMinerGameCommand(BlueprintStarSystemObject sso, BlueprintResource resource)
	{
		BlueprintSso = sso.ToReference<BlueprintStarSystemObjectReference>();
		BlueprintResource = resource.ToReference<BlueprintResourceReference>();
	}

	protected override void ExecuteInternal()
	{
		StarSystemObjectEntity sso = Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity obj) => obj.Blueprint == BlueprintSso.Get());
		if (sso == null)
		{
			PFLog.Net.Error("[UseResourceMinerGameCommand] StarSystemObjectEntity not found! " + BlueprintSso.Guid);
			return;
		}
		BlueprintResource resource = BlueprintResource;
		if (resource == null)
		{
			PFLog.Net.Error("[UseResourceMinerGameCommand] BlueprintResource not found! " + BlueprintResource.Guid);
			return;
		}
		ItemEntity itemEntity = sso.ResourceMiners[resource];
		sso.ResourceMiners.Remove(resource);
		ColoniesState.MinerData minerData = Game.Instance.Player.ColoniesState.Miners.FirstOrDefault((ColoniesState.MinerData m) => m.Sso == sso?.Blueprint && m.Resource == resource);
		if (minerData != null)
		{
			Game.Instance.Player.ColoniesState.Miners.Remove((ColoniesState.MinerData m) => m.Sso == sso.Blueprint && m.Resource == resource);
			int resourceFromMinerCountWithProductivity = ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(minerData);
			Game.Instance.Player.Inventory.Add(itemEntity.Blueprint);
			Game.Instance.ColonizationController.RemoveResourceNotFromColony(resource, resourceFromMinerCountWithProductivity);
			EventBus.RaiseEvent(delegate(IMiningUIHandler h)
			{
				h.HandleStopMining(sso, resource);
			});
		}
	}

	static RemoveResourceMinerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveResourceMinerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RemoveResourceMinerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveResourceMinerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RemoveResourceMinerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RemoveResourceMinerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.BlueprintSso);
		writer.WritePackable(in value.BlueprintResource);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RemoveResourceMinerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintStarSystemObjectReference value2;
		BlueprintResourceReference value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.BlueprintSso;
				value3 = value.BlueprintResource;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<BlueprintStarSystemObjectReference>();
			value3 = reader.ReadPackable<BlueprintResourceReference>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RemoveResourceMinerGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value.BlueprintSso;
				value3 = value.BlueprintResource;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new RemoveResourceMinerGameCommand
		{
			BlueprintSso = value2,
			BlueprintResource = value3
		};
		return;
		IL_009a:
		value.BlueprintSso = value2;
		value.BlueprintResource = value3;
	}
}
