using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
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

	[SerializeField]
	private bool m_RemoveStackOnlyFromCaster;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public bool RemoveStackOnlyFromCaster => m_RemoveStackOnlyFromCaster;

	public override string GetCaption()
	{
		return "Remove one stack of Buff: " + (TargetBuff ? TargetBuff.Name : "<not specified>");
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = (RemoveStackOnlyFromCaster ? base.Context.MaybeCaster : null);
		if (RemoveStackOnlyFromCaster && mechanicEntity == null)
		{
			PFLog.Default.Error("Context.MaybeCaster can't be null!");
			return;
		}
		BuffCollection buffCollection = base.Target?.Entity?.Buffs;
		if (buffCollection == null)
		{
			return;
		}
		Buff buff = null;
		foreach (Buff item in buffCollection)
		{
			if (item != null && item.Blueprint == TargetBuff && (!RemoveStackOnlyFromCaster || item.Context.MaybeCaster == mechanicEntity))
			{
				buff = item;
				break;
			}
		}
		buff?.Remove();
	}
}
