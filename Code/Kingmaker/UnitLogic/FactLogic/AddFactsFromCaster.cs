using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("972fa4eff9ddf1d409b76eb577b6145e")]
public class AddFactsFromCaster : UnitBuffComponentDelegate, IUnitReapplyFeaturesOnLevelUpHandler<EntitySubscriber>, IUnitReapplyFeaturesOnLevelUpHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitReapplyFeaturesOnLevelUpHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Facts")]
	private BlueprintUnitFactReference[] m_Facts;

	public bool FeatureFromSelection;

	[ShowIf("FeatureFromSelection")]
	[SerializeField]
	private BlueprintFeatureSelectionReference m_Selection;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public BlueprintFeatureSelection_Obsolete Selection => m_Selection;

	public void HandleUnitReapplyFeaturesOnLevelUp()
	{
		OnRecalculate();
	}

	protected override void OnActivate()
	{
		MechanicEntity mechanicEntity = base.Fact.MaybeContext?.MaybeCaster;
		if (mechanicEntity == null)
		{
			return;
		}
		if (!FeatureFromSelection)
		{
			foreach (BlueprintUnitFact fact in Facts)
			{
				if (mechanicEntity.Facts.Contains(fact) && base.Owner.Facts.FindBySource(fact, base.Fact, this) == null)
				{
					base.Owner.AddFact(fact, base.Context).AddSource(base.Fact, this);
				}
			}
			return;
		}
		foreach (BlueprintFeature allFeature in Selection.AllFeatures)
		{
			if (mechanicEntity.Facts.Contains(allFeature) && base.Owner.Facts.FindBySource(allFeature, base.Fact, this) == null)
			{
				base.Owner.AddFact(allFeature, base.Context).AddSource(base.Fact, this);
			}
		}
	}

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
