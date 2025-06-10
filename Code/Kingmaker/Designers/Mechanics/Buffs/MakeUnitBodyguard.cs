using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d42f85662e50421b8ab98ecb25cf9255")]
public class MakeUnitBodyguard : UnitFactComponentDelegate, IHashable
{
	[SerializeReference]
	public AbstractUnitEvaluator Defendant;

	[SerializeField]
	public bool DefendOwnerByDefendant;

	protected override void OnActivate()
	{
		if (Defendant == null)
		{
			return;
		}
		if (!(Defendant.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Fact {this}, {Defendant} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		BaseUnitEntity baseUnitEntity2 = baseUnitEntity;
		if ((bool)base.Owner.GetOptional<UnitPartBodyGuard>())
		{
			PFLog.Ability.Warning($"{base.Owner} gets new defendant as bodyguard but allready has one");
		}
		if (DefendOwnerByDefendant)
		{
			baseUnitEntity2.GetOrCreate<UnitPartBodyGuard>().Init(base.Owner);
		}
		else
		{
			base.Owner.GetOrCreate<UnitPartBodyGuard>().Init(baseUnitEntity2);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitPartBodyGuard>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
