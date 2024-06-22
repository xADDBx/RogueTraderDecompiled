using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;

namespace Kingmaker.AI;

public abstract class AbilityTargetSelector
{
	protected AbilityInfo AbilityInfo;

	public TargetWrapper SelectedTarget { get; protected set; }

	public List<MechanicEntity> AffectedTargets { get; protected set; } = new List<MechanicEntity>();


	public AbilityTargetSelector([NotNull] AbilityInfo abilityInfo)
	{
		AbilityInfo = abilityInfo;
	}

	public abstract bool HasPossibleTarget(DecisionContext context, CustomGridNodeBase casterNode);

	public abstract TargetWrapper SelectTarget(DecisionContext context, CustomGridNodeBase casterNode);

	public virtual bool IsValidTarget(TargetWrapper targetWrapper)
	{
		MechanicEntity entity = targetWrapper.Entity;
		if (entity == null)
		{
			return false;
		}
		if ((bool)entity.Features.IsUntargetable)
		{
			return false;
		}
		if (AbilityInfo.settings == null)
		{
			return true;
		}
		return AbilityInfo.settings.IsValidTarget(new PropertyContext(AbilityInfo.ability, entity));
	}

	public virtual bool IsTargetCounts(TargetWrapper targetWrapper)
	{
		MechanicEntity entity = targetWrapper.Entity;
		if (entity == null || !entity.IsConscious)
		{
			return false;
		}
		if (AbilityInfo.settings == null)
		{
			return true;
		}
		return AbilityInfo.settings.IsTargetCounts(new PropertyContext(AbilityInfo.ability, entity));
	}
}
