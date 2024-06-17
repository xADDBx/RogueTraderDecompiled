using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("2631f5df3cb69464399dcdbcbbd803d8")]
public class EtudeBracketMarkUnitEssential : EtudeBracketTrigger, IHashable
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override void OnEnter()
	{
		Target.GetValue().GetOrCreate<UnitPartEssential>().Retain();
	}

	protected override void OnExit()
	{
		Target.GetValue().GetOptional<UnitPartEssential>()?.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
