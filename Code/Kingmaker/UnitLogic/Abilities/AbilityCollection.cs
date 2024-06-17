using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Abilities;

[JsonObject]
public class AbilityCollection : MechanicEntityFactsCollection<Ability>
{
	public BaseUnitEntity Owner => (BaseUnitEntity)base.Manager.Owner;

	[JsonConstructor]
	public AbilityCollection()
	{
	}

	public IEnumerator<Ability> GetEnumerator()
	{
		return base.Enumerable.GetEnumerator();
	}

	public Ability Add(BlueprintAbility ability)
	{
		return base.Manager.Add(new Ability(ability, Owner));
	}

	[CanBeNull]
	public Ability GetAbility(BlueprintAbility blueprintAbility)
	{
		for (int i = 0; i < base.RawFacts.Count; i++)
		{
			Ability ability = base.RawFacts[i];
			if (ability.Blueprint == blueprintAbility)
			{
				return ability;
			}
		}
		return null;
	}

	protected override Ability PrepareFactForAttach(Ability fact)
	{
		return fact;
	}

	protected override Ability PrepareFactForDetach(Ability fact)
	{
		return fact;
	}

	protected override void OnFactDidAttach(Ability fact)
	{
		base.OnFactDidAttach(fact);
		if (Owner.Faction.IsPlayer)
		{
			EventBus.RaiseEvent(delegate(IPlayerAbilitiesHandler h)
			{
				h.HandleAbilityAdded(fact);
			});
		}
	}

	protected override void OnFactWillDetach(Ability fact)
	{
		base.OnFactWillDetach(fact);
		if (Owner.Faction.IsPlayer)
		{
			Owner.UISettings.RemoveSlot(fact);
			EventBus.RaiseEvent(delegate(IPlayerAbilitiesHandler h)
			{
				h.HandleAbilityRemoved(fact);
			});
		}
	}

	[CanBeNull]
	public Ability GetAbilityWithComponent<T>() where T : BlueprintComponent
	{
		return base.RawFacts.FirstItem((Ability a) => a.Blueprint.GetComponents<T>() != null);
	}

	public void OnCombatEnd()
	{
		(from i in base.Enumerable
			select (ItemEntity)i.SourceItem into i
			where i != null && i.Blueprint is BlueprintItemEquipment blueprintItemEquipment && blueprintItemEquipment.RestoreChargesAfterCombat
			select i).ForEach(delegate(ItemEntity i)
		{
			i.RestoreCharges();
		});
	}
}
