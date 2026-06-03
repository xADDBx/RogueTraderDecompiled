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

	[SerializeField]
	private bool m_DestroyAreaEffectOnlyUnderCaster;

	public BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect?.Get();

	public bool DestroyAreaEffectOnlyFromCaster => m_DestroyAreaEffectOnlyFromCaster;

	public bool DestroyAreaEffectOnlyUnderCaster => m_DestroyAreaEffectOnlyUnderCaster;

	public override string GetCaption()
	{
		string text = ((AreaEffect != null) ? AreaEffect.ToString() : "<undefined>");
		bool destroyAreaEffectOnlyFromCaster = m_DestroyAreaEffectOnlyFromCaster;
		bool destroyAreaEffectOnlyUnderCaster = m_DestroyAreaEffectOnlyUnderCaster;
		string text2 = (destroyAreaEffectOnlyFromCaster ? ((!destroyAreaEffectOnlyUnderCaster) ? " [created by caster]" : " [created by caster, under caster]") : ((!destroyAreaEffectOnlyUnderCaster) ? "" : " [under caster]"));
		string text3 = text2;
		return "Destroy " + text + text3;
	}

	protected override void RunAction()
	{
		int num;
		object obj;
		if (!DestroyAreaEffectOnlyFromCaster)
		{
			num = (DestroyAreaEffectOnlyUnderCaster ? 1 : 0);
			if (num == 0)
			{
				obj = null;
				goto IL_0022;
			}
		}
		else
		{
			num = 1;
		}
		obj = base.Context.MaybeCaster;
		goto IL_0022;
		IL_0022:
		MechanicEntity mechanicEntity = (MechanicEntity)obj;
		if (num != 0 && mechanicEntity == null)
		{
			PFLog.Default.Error("Context.MaybeCaster can't be null!");
			return;
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint == AreaEffect && (!DestroyAreaEffectOnlyFromCaster || areaEffect.Context.MaybeCaster == mechanicEntity) && (!DestroyAreaEffectOnlyUnderCaster || areaEffect.Contains(mechanicEntity.Position)))
			{
				areaEffect.ForceEnd();
			}
		}
	}
}
