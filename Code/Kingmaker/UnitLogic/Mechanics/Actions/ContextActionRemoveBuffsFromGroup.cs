using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("285b50b52a5b4192aad65da172555c50")]
public class ContextActionRemoveBuffsFromGroup : ContextAction
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public bool RemoveOneRandomBuff;

	public bool ToCaster;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	public override string GetCaption()
	{
		return string.Concat("Remove buffs from group list ");
	}

	protected override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			Element.LogError(this, "Unable to remove buff: no context found");
			return;
		}
		MechanicEntity mechanicEntity = (ToCaster ? mechanicsContext.MaybeCaster : base.Target.Entity);
		if (mechanicEntity == null)
		{
			return;
		}
		List<Buff> list = mechanicEntity.Buffs.Enumerable.Where((Buff buff) => buff.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup group) => Groups.Contains(group))).ToList();
		if (RemoveOneRandomBuff)
		{
			list.Random(PFStatefulRandom.Mechanics)?.Remove();
			return;
		}
		foreach (Buff item in list)
		{
			item.Remove();
		}
	}
}
