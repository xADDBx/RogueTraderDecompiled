using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("05fab0d4998390a4cbf88da0ab1e4123")]
public class AddMechanicsFeature : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, AddMechanicsFeature>, IHashable
	{
		protected override void OnActivateOrPostLoad()
		{
			if (!(base.Fact is UnitFact fact) || base.SourceBlueprintComponent.m_Restrictions.IsPassed(fact))
			{
				base.Owner.GetMechanicFeature(base.SourceBlueprintComponent.m_Feature).Retain(base.Fact as Buff);
			}
		}

		protected override void OnDeactivate()
		{
			if (!(base.Fact is UnitFact fact) || base.SourceBlueprintComponent.m_Restrictions.IsPassed(fact))
			{
				base.Owner.GetMechanicFeature(base.SourceBlueprintComponent.m_Feature).Release(base.Fact as Buff);
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private MechanicsFeatureType m_Feature;

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public MechanicsFeatureType Feature => m_Feature;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
