using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("bf55bed15f21464eb16c7ee6c5911e31")]
public class ContextActionStealBuffByGroup : ContextAction
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	[SerializeField]
	private bool m_CopyNotRemove;

	public override string GetCaption()
	{
		if (m_CopyNotRemove)
		{
			return "Duplicates group buffs to itself without removing them from the target";
		}
		return "Duplicates group buffs to itself and removing them from the target";
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = ContextData<MechanicsContext.Data>.Current?.Context?.MaybeCaster;
		if (mechanicEntity == null)
		{
			Element.LogError(this, "ContextActionStealBuffByGroup: Caster is null");
			return;
		}
		MechanicEntity mechanicEntity2 = base.Target?.Entity;
		if (mechanicEntity2 == null)
		{
			Element.LogError(this, "ContextActionStealBuffByGroup: Target is null");
			return;
		}
		BlueprintReference<BlueprintAbilityGroup>[] groups2 = m_Groups;
		ReferenceArrayProxy<BlueprintAbilityGroup> groups = groups2;
		foreach (Buff item in mechanicEntity2.Buffs.Enumerable.Where((Buff buff) => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup group) => groups.Contains(group))).ToList())
		{
			Buff buff2 = mechanicEntity.Buffs.Add(item.Blueprint);
			if (buff2 != null)
			{
				buff2.AddRank(item.Rank - buff2.Rank);
				buff2.SetDuration(item.ExpirationInRounds);
			}
			if (!m_CopyNotRemove)
			{
				item.Remove();
			}
		}
	}
}
