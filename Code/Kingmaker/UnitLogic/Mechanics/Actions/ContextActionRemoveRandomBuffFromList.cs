using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("b05ba4253f59f2b4081b680ce11b0206")]
public class ContextActionRemoveRandomBuffFromList : ContextAction
{
	private enum CasterRanksRemovalPolicy
	{
		Default,
		ByOne,
		All
	}

	[SerializeField]
	private BlueprintBuffReference[] m_Buffs = new BlueprintBuffReference[0];

	public bool RemoveRank;

	public bool ToCaster;

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	public override string GetCaption()
	{
		return string.Concat("Remove random Buff from list");
	}

	public override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			PFLog.Default.Error(this, "Unable to remove buff: no context found");
			return;
		}
		MechanicEntity target = (ToCaster ? mechanicsContext.MaybeCaster : base.Target.Entity);
		IEnumerable<BlueprintBuff> source = Buffs.Where((BlueprintBuff p) => target.Buffs.Contains(p));
		if (source.Count() != 0)
		{
			PFStatefulRandom.Mechanics.Range(0, source.Count() - 1);
			BlueprintBuff blueprint = source.ToList().Random(PFStatefulRandom.Mechanics);
			target?.Buffs.Remove(blueprint);
		}
	}
}
