using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem;

public static class EntityFactsManagerHelper
{
	[CanBeNull]
	public static EntityFact FindById(this EntityFactsManager manager, string factId)
	{
		return manager.List.FirstItem((EntityFact f) => f.UniqueId == factId);
	}

	[CanBeNull]
	public static EntityFact FindBySource(this EntityFactsManager manager, BlueprintFact blueprint, EntityFact fact, BlueprintComponent component = null)
	{
		return manager.List.FirstItem((EntityFact f) => f.Blueprint == blueprint && f.IsFrom(fact, component));
	}

	[CanBeNull]
	public static EntityFact FindBySource(this EntityFactsManager manager, BlueprintFact blueprint, Entity entity)
	{
		return manager.List.FirstItem((EntityFact f) => f.Blueprint == blueprint && f.IsFrom(entity));
	}

	[CanBeNull]
	public static EntityFact FindBySource(this EntityFactsManager manager, BlueprintFact blueprint, UnitCondition unitCondition)
	{
		return manager.List.FirstItem((EntityFact f) => f.Blueprint == blueprint && f.IsFrom(unitCondition));
	}

	[CanBeNull]
	public static EntityFact FindBySource(this EntityFactsManager manager, BlueprintFact blueprint, Etude etude)
	{
		return manager.List.FirstItem((EntityFact f) => f.Blueprint == blueprint && f.IsFrom(etude));
	}

	public static IEnumerable<EntityFact> FindAllBySource(this EntityFactsManager manager, EntityFact fact, BlueprintComponent component = null)
	{
		return manager.List.Where((EntityFact f) => f.IsFrom(fact, component));
	}
}
