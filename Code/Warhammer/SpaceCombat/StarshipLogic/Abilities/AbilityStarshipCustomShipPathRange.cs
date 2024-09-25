using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Pointer.AbilityTarget;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

public class AbilityStarshipCustomShipPathRange : AbilityRange, IShowAoEAffectedUIHandler, ISubscriber
{
	[SerializeField]
	private GameObject DefaultMarker;

	private ICustomShipPathProvider CustomPathProvider => Ability.Blueprint.GetComponent<ICustomShipPathProvider>();

	protected override bool CanEnable()
	{
		if (base.CanEnable())
		{
			return CustomPathProvider != null;
		}
		return false;
	}

	protected override void SetFirstSpecs()
	{
		if (Ability.Caster is StarshipEntity starshipEntity)
		{
			Vector3 desiredPosition = Game.Instance.VirtualPositionController.GetDesiredPosition(starshipEntity);
			Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
			Dictionary<GraphNode, CustomPathNode> customPath = CustomPathProvider.GetCustomPath(starshipEntity, desiredPosition, currentUnitDirection);
			Game.Instance.StarshipPathController.ShowCustomShipPath(customPath, DefaultMarker);
		}
	}

	protected override void SetRangeToWorldPosition(Vector3 castPosition)
	{
		if (Ability.Caster is StarshipEntity starship)
		{
			Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
			List<CustomGridNodeBase> list = CustomPathProvider.GetCustomPath(starship, castPosition, currentUnitDirection).Keys.Select((GraphNode x) => x as CustomGridNodeBase).ToTempList();
			OrientedPatternData pattern = new OrientedPatternData(list, list.FirstOrDefault());
			NodeList nodes = Ability.Caster.GetOccupiedNodes(Ability.Caster.Position);
			if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
			{
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
				abilityAreaHudInfo.pattern = pattern;
				abilityAreaHudInfo.casterRect = result;
				abilityAreaHudInfo.minRange = Ability.MinRangeCells;
				abilityAreaHudInfo.maxRange = Ability.RangeCells;
				abilityAreaHudInfo.effectiveRange = 0;
				abilityAreaHudInfo.ignoreRangesByDefault = false;
				abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault = false;
				abilityAreaHudInfo.combatHudCommandsOverride = Ability.Blueprint.CombatHudCommandsOverride;
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHUD = abilityAreaHudInfo;
				CombatHUDRenderer.Instance.SetAbilityAreaHUD(abilityAreaHUD);
			}
		}
	}

	public void HandleAoEMove(Vector3 pos, AbilityData ability)
	{
	}

	public void HandleAoECancel()
	{
		Game.Instance.StarshipPathController.ShowCurrentShipPath();
	}
}
