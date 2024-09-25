using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("4cac5a9f3f7f9c5439c2cac79a95406a")]
public class EtudeBracketDetachPet : EtudeBracketTrigger, IHashable
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Master;

	public PetType PetType;

	protected override void OnEnter()
	{
	}

	protected override void OnExit()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
