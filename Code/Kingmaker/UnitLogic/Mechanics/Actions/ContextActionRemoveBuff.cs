using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("23ddfb172c2d3c144ab007dec380d712")]
public class ContextActionRemoveBuff : ContextAction
{
	private enum CasterRanksRemovalPolicy
	{
		Default,
		ByOne,
		All
	}

	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	public bool RemoveRank;

	[ShowIf("RemoveRank")]
	public bool RemoveSeveralRanks;

	[ShowIf("RemoveSeveralRanks")]
	public ContextValue RankNumber;

	[SerializeField]
	[ShowIf("RemoveRank")]
	private CasterRanksRemovalPolicy m_CasterRanksRemovalPolicy;

	public bool ToCaster;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetCaption()
	{
		return "Remove Buff: " + Buff.NameSafe();
	}

	public override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			PFLog.Default.Error(this, "Unable to remove buff: no context found");
			return;
		}
		MechanicEntity mechanicEntity = (ToCaster ? mechanicsContext.MaybeCaster : base.Target.Entity);
		using (ContextData<BuffCollection.RemoveByRank>.RequestIf(RemoveRank))
		{
			using (ContextData<CasterUnitData>.RequestIf(m_CasterRanksRemovalPolicy != 0 && mechanicsContext.MaybeCaster is BaseUnitEntity)?.Setup((BaseUnitEntity)mechanicsContext.MaybeCaster))
			{
				if (RemoveSeveralRanks)
				{
					int count = RankNumber.Calculate(base.Context);
					mechanicEntity?.Buffs.GetBuff(Buff)?.RemoveRank(count);
				}
				else if (m_CasterRanksRemovalPolicy == CasterRanksRemovalPolicy.All)
				{
					RemoveAllRanksFromCaster(mechanicEntity, mechanicsContext);
				}
				else
				{
					mechanicEntity?.Buffs.Remove(Buff);
				}
			}
		}
	}

	private void RemoveAllRanksFromCaster(MechanicEntity target, MechanicsContext context)
	{
		List<Buff> list = TempList.Get<Buff>();
		list.AddRange(target.Buffs.RawFacts.Where((Buff i) => i.Blueprint == Buff && i.Context.MaybeCaster == context.MaybeCaster));
		foreach (Buff item in list)
		{
			item.Remove();
		}
	}
}
