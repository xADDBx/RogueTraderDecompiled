using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Buff on spawned unit")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("305101c17be4ab540ba629cb531dea3b")]
[AllowMultipleComponents]
public class OnSpawnBuff : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSummonUnit>, IRulebookHandler<RulePerformSummonUnit>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[InfoBox("Caster's Fact")]
	[SerializeField]
	[FormerlySerializedAs("IfHaveFact")]
	private BlueprintFeatureReference m_IfHaveFact;

	public bool CheckSummonedUnitFact;

	[ShowIf("CheckSummonedUnitFact")]
	[SerializeField]
	private BlueprintFeatureReference m_IfSummonHaveFact;

	[SerializeField]
	[FormerlySerializedAs("buff")]
	private BlueprintBuffReference m_buff;

	public bool CheckDescriptor;

	[ShowIf("CheckDescriptor")]
	public SpellDescriptorWrapper SpellDescriptor;

	public bool IsInfinity;

	[HideIf("IsInfinity")]
	public Rounds duration;

	public BlueprintFeature IfHaveFact => m_IfHaveFact?.Get();

	public BlueprintFeature IfSummonHaveFact => m_IfSummonHaveFact?.Get();

	public BlueprintBuff buff => m_buff?.Get();

	public void OnEventAboutToTrigger(RulePerformSummonUnit evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSummonUnit evt)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
		Rounds? rounds = (IsInfinity ? null : new Rounds?(duration));
		if ((mechanicEntity.Facts.Contains(IfHaveFact) || IfHaveFact == null) && (!CheckDescriptor || (evt.Reason.Context.SpellDescriptor & SpellDescriptor) != Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells.SpellDescriptor.None) && (!CheckSummonedUnitFact || evt.SummonedUnit.Facts.Contains(IfSummonHaveFact)))
		{
			evt.SummonedUnit.Buffs.Add(buff, mechanicEntity, rounds);
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
