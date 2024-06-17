using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("79321e7cc8830c2459f5aef8d5f0d530")]
public class ContextActionRemoveBuffSingleStack : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("TargetBuff")]
	private BlueprintBuffReference m_TargetBuff;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public override string GetCaption()
	{
		return "Remove one stack of Buff: " + (TargetBuff ? TargetBuff.Name : "<not specified>");
	}

	public override void RunAction()
	{
		base.Target.Entity.Buffs.GetBuff(TargetBuff)?.Remove();
	}
}
