using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Components.Fixers;

[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("a6f2373d0e9f489aaf1e5e35b51e0411")]
public abstract class FixUnitOnPostLoad : BlueprintComponent
{
	[UsedImplicitly]
	public string TaskId;

	[UsedImplicitly]
	public string Comment;

	public abstract void OnPostLoad(BaseUnitEntity unit);
}
