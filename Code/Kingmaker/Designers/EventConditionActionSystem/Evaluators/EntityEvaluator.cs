using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[PlayerUpgraderAllowed(true)]
[TypeId("73b2a1364e934404b5bc0b56bb6c9c6f")]
public abstract class EntityEvaluator : GenericEvaluator<Entity>
{
}
