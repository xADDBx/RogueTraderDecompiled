using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
public abstract class UnitFactComponentDelegate : EntityFactComponentDelegate<BaseUnitEntity>, IHashable
{
	protected ITargetWrapper OwnerTargetWrapper => base.Owner.ToITargetWrapper();

	protected new UnitFact Fact
	{
		[return: NotNull]
		get
		{
			return (base.Fact as UnitFact) ?? throw new Exception($"Component on invalid fact: {base.Fact.Blueprint}.{GetType().Name}");
		}
	}

	protected FeatureParam Param => ComponentEventContext.CurrentRuntime.Param;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
