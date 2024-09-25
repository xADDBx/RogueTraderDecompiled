using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("64620c7c30264f80ae8665622037a228")]
public class WarhammerBrainPriorityBonus : EntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private ScorePair m_ScorePairToForce;

	protected override void OnActivate()
	{
		base.Owner.GetOptional<PartUnitBrain>()?.ForceScorePairPriority(m_ScorePairToForce);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartUnitBrain>()?.ResetScorePairPriority();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
