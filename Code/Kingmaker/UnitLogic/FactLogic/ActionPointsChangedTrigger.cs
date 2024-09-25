using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("9cec0bfbc21542f4a1690b6bfc8c3ab6")]
public abstract class ActionPointsChangedTrigger : UnitFactComponentDelegate, IHashable
{
	protected enum PointsType
	{
		Yellow,
		Blue,
		Any
	}

	[SerializeField]
	protected PointsType m_Type;

	public RestrictionCalculator Restriction;

	public ActionList Actions;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
