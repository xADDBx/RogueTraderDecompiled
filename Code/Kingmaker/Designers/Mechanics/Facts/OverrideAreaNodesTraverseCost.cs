using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbilityAreaEffect))]
[AllowMultipleComponents]
[TypeId("6fd810e1ab8b4953bba5e760e4e7087e")]
public class OverrideAreaNodesTraverseCost : EntityFactComponentDelegate<AreaEffectEntity>, IAreaHandler, ISubscriber, IHashable
{
	[SerializeField]
	[CanBeNull]
	private RestrictionsHolder.Reference m_UnitRestriction;

	[SerializeField]
	private PropertyCalculator m_CostProc;

	protected override void OnActivate()
	{
		AddOverride();
	}

	protected override void OnDeactivate()
	{
		RemoveOverride();
	}

	protected override void OnDispose()
	{
		RemoveOverride();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		AddOverride();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		RemoveOverride();
	}

	private void AddOverride()
	{
		EntityFactSource source = GetSource();
		PropertyContext valueCalculationContext = GetValueCalculationContext();
		int overridePercentCost = m_CostProc?.GetValue(valueCalculationContext) ?? 0;
		NodeTraverseCostHelper.AddOverrideCost(source, overridePercentCost, m_UnitRestriction);
	}

	private void RemoveOverride()
	{
		NodeTraverseCostHelper.RemoveOverrideCost(GetSource());
	}

	private EntityFactSource GetSource()
	{
		return new EntityFactSource(base.Owner);
	}

	private PropertyContext GetValueCalculationContext()
	{
		return new PropertyContext(base.Owner, null).WithContext(base.Context);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
