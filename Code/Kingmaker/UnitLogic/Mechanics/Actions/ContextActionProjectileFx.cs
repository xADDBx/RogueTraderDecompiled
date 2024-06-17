using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("eb117305cabfd9b4c9d44512730470bf")]
public class ContextActionProjectileFx : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Projectile")]
	private BlueprintProjectileReference m_Projectile;

	public new BlueprintProjectile Projectile => m_Projectile?.Get();

	public override string GetCaption()
	{
		return "Spawn Projectile FX: " + ((Projectile != null) ? Projectile.name : "unspecified");
	}

	public override void RunAction()
	{
		if (!base.Context.DisableFx)
		{
			if (base.Context.MaybeCaster == null)
			{
				PFLog.Default.Error(this, "Caster is missing");
			}
			else
			{
				new ProjectileLauncher(Projectile, base.Context.MaybeCaster, base.Target).Ability(base.AbilityContext?.Ability).Launch();
			}
		}
	}
}
