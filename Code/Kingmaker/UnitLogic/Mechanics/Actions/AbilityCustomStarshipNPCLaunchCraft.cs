using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("456c918b2de179347b1d6e2fbd3a5bf5")]
public class AbilityCustomStarshipNPCLaunchCraft : AbilityCustomLogic, IAbilityCasterRestriction
{
	private class TransientData : IEntityFactComponentTransientData
	{
		public int[] cooldowns;
	}

	[SerializeField]
	private ActionList m_ActionsOnUnit;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.MaybeCaster is StarshipEntity starship))
		{
			yield break;
		}
		StarshipLaunchBayLogic bayLogic = starship.Facts.GetComponents<StarshipLaunchBayLogic>().FirstOrDefault();
		if (bayLogic == null)
		{
			yield break;
		}
		TargetWrapper resultTarget = starship;
		while (true)
		{
			var (num, spawnInfo) = bayLogic.GetBestBayIndex(starship, target.Point);
			if (num < 0)
			{
				break;
			}
			BaseUnitEntity baseUnitEntity = WarhammerContextActionSpawnChildStarship.SpawnStarship(spawnInfo.bayInfo.WingBlueprint, spawnInfo.position, spawnInfo.rotation, starship, actBeforeSummoner: false);
			using (context.GetDataScope(baseUnitEntity.ToITargetWrapper()))
			{
				m_ActionsOnUnit?.Run();
			}
			bayLogic.LaunchBay(starship, num);
			resultTarget = baseUnitEntity;
			double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
			while (!((float)(Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000f >= spawnInfo.bayInfo.launchTime))
			{
				yield return null;
			}
		}
		yield return new AbilityDeliveryTarget(resultTarget);
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (caster is StarshipEntity starshipEntity)
		{
			StarshipLaunchBayLogic starshipLaunchBayLogic = starshipEntity.Facts.GetComponents<StarshipLaunchBayLogic>().FirstOrDefault();
			if (starshipLaunchBayLogic != null)
			{
				return starshipLaunchBayLogic.GetBestBayIndex(starshipEntity, starshipEntity.Position).index >= 0;
			}
		}
		return false;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.CantLaunchMoreWings;
	}
}
