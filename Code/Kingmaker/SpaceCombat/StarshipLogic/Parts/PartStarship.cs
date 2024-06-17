using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarship : BaseUnitPart, IAreaHandler, ISubscriber, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarship>, IEntityPartOwner
	{
		PartStarship Starship { get; }
	}

	[NotNull]
	private BlueprintStarship Blueprint => (BlueprintStarship)base.Owner.Blueprint;

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	private PartUnitCombatState CombatState => base.Owner.GetRequired<PartUnitCombatState>();

	public int Inertia => StatsContainer.GetStat(StatType.Inertia);

	public int Evasion => StatsContainer.GetStat(StatType.Evasion);

	public ModifiableValue Crew => StatsContainer.GetStat(StatType.Crew);

	public ModifiableValue TurretRating => StatsContainer.GetStat(StatType.TurretRating);

	public ModifiableValue TurretRadius => StatsContainer.GetStat(StatType.TurretRadius);

	public ModifiableValue MilitaryRating => StatsContainer.GetStat(StatType.MilitaryRating);

	protected override void OnAttach()
	{
		InitializeStats();
	}

	protected override void OnPrePostLoad()
	{
		InitializeStats();
	}

	private void InitializeStats()
	{
		StatsContainer.Register(StatType.ArmourFore);
		StatsContainer.Register(StatType.ArmourPort);
		StatsContainer.Register(StatType.ArmourStarboard);
		StatsContainer.Register(StatType.ArmourAft);
		StatsContainer.Register(StatType.Inertia);
		StatsContainer.Register(StatType.Evasion);
		StatsContainer.Register(StatType.Morale);
		StatsContainer.Register(StatType.Crew);
		StatsContainer.Register(StatType.TurretRating);
		StatsContainer.Register(StatType.TurretRadius);
		StatsContainer.Register(StatType.MilitaryRating);
	}

	private void ChooseVisual()
	{
		BlueprintStarshipStarSystemSettingsComponent component = base.Owner.Blueprint.GetComponent<BlueprintStarshipStarSystemSettingsComponent>();
		if (component != null && !(component.StarSystemVisual == null) && !(component.BaseVisual == null))
		{
			if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem)
			{
				component.StarSystemVisual.SetActive(value: true);
				component.BaseVisual.SetActive(value: false);
			}
			else if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
			{
				component.StarSystemVisual.SetActive(value: false);
				component.BaseVisual.SetActive(value: true);
			}
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		ChooseVisual();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
