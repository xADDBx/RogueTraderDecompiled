using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Cargo;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic;

public class StarshipItemEntity<T> : ItemEntity<T>, IHashable where T : BlueprintStarshipItem
{
	[JsonProperty]
	public bool IsBroken;

	public override bool CanBeAssembled => true;

	public override bool CanBeDisassembled => true;

	public StarshipItemEntity(T bpItem)
		: base(bpItem)
	{
		IsBroken = base.Blueprint.IsBroken;
	}

	public StarshipItemEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public override bool Assemble()
	{
		BlueprintStarshipItem blueprint = base.Blueprint;
		if (blueprint == null)
		{
			return false;
		}
		int assembleItemRequiredScrap = blueprint.AssembleItemRequiredScrap;
		List<BlueprintPartsCargoReference> assembleItemRequirements = blueprint.AssembleItemRequirements;
		if ((int)Game.Instance.Player.Scrap < assembleItemRequiredScrap)
		{
			return false;
		}
		List<CargoEntity> source = Game.Instance.Player.CargoState.CargoEntities.Where((CargoEntity x) => x.Blueprint is BlueprintPartsCargo).ToList();
		List<CargoEntity> list = new List<CargoEntity>();
		foreach (BlueprintPartsCargoReference part in assembleItemRequirements)
		{
			CargoEntity cargoEntity = source.FirstOrDefault((CargoEntity x) => (x.Blueprint as BlueprintPartsCargo)?.Type == part.Get().Type);
			if (cargoEntity == null)
			{
				return false;
			}
			list.Add(cargoEntity);
		}
		foreach (CargoEntity item in list)
		{
			Game.Instance.Player.CargoState.Remove(item);
		}
		Game.Instance.Player.Scrap.Spend(assembleItemRequiredScrap);
		IsBroken = false;
		return true;
	}

	public override bool Disassemble()
	{
		BlueprintStarshipItem blueprint = base.Blueprint;
		if (blueprint == null)
		{
			return false;
		}
		int disassembleScrapGiven = blueprint.DisassembleScrapGiven;
		Game.Instance.Player.Scrap.Receive(disassembleScrapGiven);
		Game.Instance.Player.Inventory.Remove(this);
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsBroken);
		return result;
	}
}
