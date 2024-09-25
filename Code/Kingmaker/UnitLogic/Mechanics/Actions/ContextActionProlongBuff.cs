using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("3f86389a846d484dbd94775da12bc94b")]
public class ContextActionProlongBuff : ContextAction
{
	public ContextDurationValue DurationValue;

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		foreach (Buff item in entity.Buffs.Enumerable.Where((Buff p) => p.Blueprint == Buff))
		{
			item.Prolong(DurationValue.Calculate(base.Context));
		}
	}

	public override string GetCaption()
	{
		return "Prolong buff duration";
	}
}
