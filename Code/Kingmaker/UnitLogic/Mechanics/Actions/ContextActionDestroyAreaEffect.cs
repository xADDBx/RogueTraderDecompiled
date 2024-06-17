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

	public BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect?.Get();

	public override string GetCaption()
	{
		string text = ((AreaEffect != null) ? AreaEffect.ToString() : "<undefined>");
		return "Destroy " + text + " ";
	}

	public override void RunAction()
	{
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint == AreaEffect)
			{
				areaEffect.ForceEnd();
			}
		}
	}
}
