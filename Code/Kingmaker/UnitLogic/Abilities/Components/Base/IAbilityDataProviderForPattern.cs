using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityDataProviderForPattern
{
	MechanicEntity Caster { get; }

	int RangeCells { get; }

	int BurstAttacksCount { get; }

	bool IsScatter { get; }

	ItemEntityStarshipWeapon StarshipWeapon { get; }

	ReadonlyList<RuleCalculateScatterShotHitDirectionProbability> ScatterShotHitDirectionProbabilities { get; }

	AbilityData Data { get; }

	CustomGridNodeBase GetBestShootingPosition(CustomGridNodeBase castNode, TargetWrapper target);

	float CalculateDodgeChanceCached(UnitEntity unit, LosCalculations.CoverType coverType);

	bool HasLosCached(CustomGridNodeBase fromNode, CustomGridNodeBase toNode);
}
