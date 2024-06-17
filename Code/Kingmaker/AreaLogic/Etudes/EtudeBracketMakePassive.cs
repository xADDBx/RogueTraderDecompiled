using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("839317076a554a999fd5def8820dd93d")]
public class EtudeBracketMakePassive : EtudeBracketTrigger, IHashable
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			Unit.GetValue().Passive.Retain();
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	protected override void OnExit()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			Unit.GetValue().Passive.Release();
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
