using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("e3295245e15c4d578cb677e5958ed9fa")]
public class OverrideUnitAsksList : EntityFactComponentDelegate<BaseUnitEntity>, IHashable
{
	[SerializeField]
	private ConditionsChecker m_Condition;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitAsksListReference m_Asks;

	private BlueprintUnitAsksList Asks => m_Asks?.Get();

	private bool CanOverride()
	{
		if (m_Condition != null)
		{
			return m_Condition.Check();
		}
		return true;
	}

	protected override void OnActivateOrPostLoad()
	{
		base.OnActivateOrPostLoad();
		Override(CanOverride() ? Asks : null);
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		Override(null);
	}

	private void Override(BlueprintUnitAsksList asksList)
	{
		base.Owner.Asks.SetOverride(asksList);
		if (base.Owner.View != null)
		{
			base.Owner.View.UpdateAsks();
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
