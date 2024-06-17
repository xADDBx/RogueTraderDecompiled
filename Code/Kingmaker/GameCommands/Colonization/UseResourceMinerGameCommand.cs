using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class UseResourceMinerGameCommand : GameCommand, IMemoryPackable<UseResourceMinerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UseResourceMinerGameCommandFormatter : MemoryPackFormatter<UseResourceMinerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UseResourceMinerGameCommand value)
		{
			UseResourceMinerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UseResourceMinerGameCommand value)
		{
			UseResourceMinerGameCommand.Deserialize(ref reader, ref value);
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
	private UseResourceMinerGameCommand()
	{
	}

	[JsonConstructor]
	public UseResourceMinerGameCommand(BlueprintStarSystemObject sso, BlueprintResource resource)
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
		BlueprintResource blueprintResource = BlueprintResource;
		if (blueprintResource == null)
		{
			PFLog.Net.Error("[UseResourceMinerGameCommand] BlueprintResource not found! " + BlueprintResource.Guid);
		}
		else
		{
			if (!sso.ResourcesOnObject.TryGetValue(blueprintResource, out var value) || sso.ResourceMiners.ContainsKey(blueprintResource))
			{
				return;
			}
			ItemEntity itemEntity = Game.Instance.Player.Inventory.Items.FirstOrDefault((ItemEntity item) => item.Blueprint.ItemType == ItemsItemType.ResourceMiner);
			if (itemEntity != null)
			{
				Game.Instance.Player.Inventory.Remove(itemEntity, 1);
				sso.ResourceMiners.Add(blueprintResource, itemEntity);
				ColoniesState.MinerData minerData = new ColoniesState.MinerData
				{
					Sso = sso.Blueprint,
					Resource = blueprintResource,
					InitialCount = value
				};
				Game.Instance.Player.ColoniesState.Miners.Add(minerData);
				Game.Instance.ColonizationController.AddResourceFromMiner(minerData, blueprintResource);
				EventBus.RaiseEvent(delegate(IMiningUIHandler h)
				{
					h.HandleStartMining(sso, blueprintResource);
				});
			}
		}
	}

	static UseResourceMinerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UseResourceMinerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new UseResourceMinerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UseResourceMinerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UseResourceMinerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UseResourceMinerGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref UseResourceMinerGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UseResourceMinerGameCommand), 2, memberCount);
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
		value = new UseResourceMinerGameCommand
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
