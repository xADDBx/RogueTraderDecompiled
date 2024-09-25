using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartUnitAsks : EntityPart<AbstractUnitEntity>, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitAsks>, IEntityPartOwner
	{
		PartUnitAsks Asks { get; }
	}

	[JsonProperty]
	[CanBeNull]
	public BlueprintUnitAsksList CustomAsks { get; private set; }

	[JsonProperty]
	[CanBeNull]
	public BlueprintUnitAsksList OverrideAsks { get; private set; }

	public BlueprintUnitAsksList List
	{
		get
		{
			if (OverrideAsks != null)
			{
				return OverrideAsks;
			}
			if (CustomAsks != null)
			{
				return CustomAsks;
			}
			return base.Owner.Blueprint.VisualSettings.Barks;
		}
	}

	protected override void OnAttach()
	{
		SpawningData current = ContextData<SpawningData>.Current;
		if (current != null)
		{
			SetCustom(current.Voice);
		}
	}

	public void SetCustom(BlueprintUnitAsksList asksList)
	{
		CustomAsks = asksList;
	}

	public void SetOverride(BlueprintUnitAsksList asksList)
	{
		OverrideAsks = asksList;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(CustomAsks);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(OverrideAsks);
		result.Append(ref val3);
		return result;
	}
}
