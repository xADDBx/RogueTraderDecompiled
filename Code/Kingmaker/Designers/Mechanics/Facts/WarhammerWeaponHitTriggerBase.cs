using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("c724a08e76a9485ab867a60389466524")]
public abstract class WarhammerWeaponHitTriggerBase : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionOnSelfHit;

	public ActionList ActionOnSelfMiss;

	public ActionList ActionsOnTargetHit;

	public ActionList ActionsOnTargetMiss;

	protected void TryRunActions(MechanicEntity initiator, MechanicEntity target, bool isHit)
	{
		if (isHit)
		{
			base.Fact.RunActionInContext(ActionOnSelfHit, initiator.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnTargetHit, target.ToITargetWrapper());
		}
		if (!isHit)
		{
			base.Fact.RunActionInContext(ActionOnSelfMiss, initiator.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnTargetMiss, target.ToITargetWrapper());
		}
		base.ExecutesCount++;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
