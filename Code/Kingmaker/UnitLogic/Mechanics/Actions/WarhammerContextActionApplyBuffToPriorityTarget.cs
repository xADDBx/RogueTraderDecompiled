using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("2de0227f2d0b4e8994f31f8401c579aa")]
public class WarhammerContextActionApplyBuffToPriorityTarget : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public bool Permanent;

	public ContextDurationValue DurationValue;

	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public BlueprintBuff Buff => m_Buff?.Get();

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public override void RunAction()
	{
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (mechanicsContext == null)
		{
			PFLog.Default.Error(this, "Unable to apply buff: no context found");
			return;
		}
		BaseUnitEntity baseUnitEntity = mechanicsContext.MaybeCaster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		if (baseUnitEntity == null)
		{
			PFLog.Default.Error(this, "Can't apply buff: target is null");
			return;
		}
		Rounds? rounds = (Permanent ? null : new Rounds?(DurationValue.Calculate(mechanicsContext)));
		baseUnitEntity.Buffs.Add(Buff, mechanicsContext, rounds);
	}

	public override string GetCaption()
	{
		string text = "Apply Buff to priority target: " + (Buff.NameSafe() ?? "???");
		if (Permanent)
		{
			return text + " (permanent)";
		}
		string text2 = DurationValue.ToString();
		return text + " (for " + text2 + ")";
	}
}
