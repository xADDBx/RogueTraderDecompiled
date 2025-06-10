using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("f2edd3ea3b1f38b429fdca720c313e95")]
public class ContextActionDestroyAreaEffect : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("AreaEffect")]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	[SerializeField]
	private bool m_DestroyAreaEffectOnlyFromCaster;

	public BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect?.Get();

	public bool DestroyAreaEffectOnlyFromCaster => m_DestroyAreaEffectOnlyFromCaster;

	public override string GetCaption()
	{
		return string.Concat("Destroy " + (m_DestroyAreaEffectOnlyFromCaster ? "only casters " : "all "), (AreaEffect != null) ? AreaEffect.ToString() : "<undefined>");
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = (DestroyAreaEffectOnlyFromCaster ? base.Context.MaybeCaster : null);
		if (DestroyAreaEffectOnlyFromCaster && mechanicEntity == null)
		{
			PFLog.Default.Error("Context.MaybeCaster can't be null!");
			return;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint == AreaEffect && (!DestroyAreaEffectOnlyFromCaster || areaEffect.Context.MaybeCaster == mechanicEntity))
			{
				areaEffect.ForceEnd();
			}
		}
	}
}
