using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Controllers.SpaceCombat;

public class StarshipPostController : IController, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>
{
	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (!(context.Caster is StarshipEntity starshipEntity) || starshipEntity != Game.Instance.Player.PlayerShip)
		{
			return;
		}
		BlueprintAbility blueprint = context.Ability.Blueprint;
		BlueprintAbility ability = blueprint.Parent ?? blueprint;
		starshipEntity.Hull.Posts.FirstOrDefault((Post data) => (data.CurrentAbilities()?.Select((Ability a) => a?.Blueprint)?.ToList()?.Contains(ability)).GetValueOrDefault())?.UnitUseAbilityFirstTime(ability);
	}

	public void HandleRecruit()
	{
	}

	public void HandleUnrecruit()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit != null)
		{
			Game.Instance.Player.PlayerShip.Hull.Posts.FirstOrDefault((Post data) => data.CurrentUnit == unit)?.SetUnitOnPost(null);
		}
	}
}
