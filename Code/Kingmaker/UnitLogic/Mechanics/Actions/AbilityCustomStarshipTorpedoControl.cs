using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("00a08a99fe7dc8c4fafa405d328c14c4")]
public class AbilityCustomStarshipTorpedoControl : AbilityCustomLogic, IAbilityCasterRestriction
{
	[SerializeField]
	private BlueprintBuffReference m_TorpedoBuff;

	[SerializeField]
	private bool AllowFirstTurn;

	public ActionList ActionsOnTorpedo;

	public ActionList ActionsOnFinish;

	public BlueprintBuff TorpedoBuff => m_TorpedoBuff?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.MaybeCaster is StarshipEntity caster))
		{
			yield break;
		}
		TargetWrapper torpedoTarget = GetTorpedoTarget(caster);
		if ((object)torpedoTarget != null)
		{
			using (context.GetDataScope(torpedoTarget))
			{
				ActionsOnTorpedo.Run();
			}
			ActionsOnFinish.Run();
			yield return new AbilityDeliveryTarget(torpedoTarget);
		}
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NoActiveTorpedo;
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity caster2))
		{
			return false;
		}
		return GetTorpedoTarget(caster2) != null;
	}

	private TargetWrapper GetTorpedoTarget(StarshipEntity caster)
	{
		List<StarshipEntity> torpedoes = new List<StarshipEntity>();
		caster.CombatGroup.ForEach(delegate(BaseUnitEntity u)
		{
			if (u is StarshipEntity)
			{
				Buff buff = u.Buffs.GetBuff(TorpedoBuff);
				if (buff != null && (AllowFirstTurn || buff.RoundNumber > 0))
				{
					torpedoes.Add(u as StarshipEntity);
				}
			}
		});
		return torpedoes.MinBy((StarshipEntity torpedo) => (torpedo.Position - caster.Position).sqrMagnitude);
	}
}
