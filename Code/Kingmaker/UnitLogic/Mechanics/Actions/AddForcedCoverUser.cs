using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("dbfcd291dd02ee24683ee62b4171ab53")]
public class AddForcedCoverUser : UnitBuffComponentDelegate, IHashable
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override void OnActivateOrPostLoad()
	{
		if (Unit.GetValue() is BaseUnitEntity user)
		{
			base.Owner.GetOrCreate<PartProvidesFullCover>().AddExclusiveUser(user);
		}
	}

	protected override void OnDeactivate()
	{
		if (Unit.GetValue() is BaseUnitEntity user)
		{
			base.Owner.GetOrCreate<PartProvidesFullCover>().RemoveExclusiveUser(user);
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
