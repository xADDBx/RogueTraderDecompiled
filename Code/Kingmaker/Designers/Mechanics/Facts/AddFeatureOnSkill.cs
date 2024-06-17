using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("73e5fab80d8f9b24c9cfb77d3fcba656")]
public class AddFeatureOnSkill : UnitFactComponentDelegate, IUnitGainPathRankHandler<EntitySubscriber>, IUnitGainPathRankHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitGainPathRankHandler, EntitySubscriber>, IHashable
{
	public StatType StatType;

	public int MinimalStat;

	[ValidateNoNullEntries]
	[SerializeField]
	[FormerlySerializedAs("Facts")]
	private BlueprintUnitFactReference[] m_Facts = new BlueprintUnitFactReference[0];

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected override void OnActivate()
	{
		TryApply();
	}

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public void HandleUnitGainPathRank(BlueprintPath path)
	{
		TryApply();
	}

	private void TryApply()
	{
		if (base.Owner.Stats.GetStat(StatType).PermanentValue < MinimalStat)
		{
			return;
		}
		foreach (BlueprintUnitFact fact in Facts)
		{
			if (base.Owner.Facts.FindBySource(fact, base.Fact, this) == null)
			{
				base.Owner.AddFact(fact).AddSource(base.Fact, this);
			}
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
