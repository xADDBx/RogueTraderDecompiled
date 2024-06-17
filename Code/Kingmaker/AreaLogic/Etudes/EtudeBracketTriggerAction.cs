using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("fae54e860aa242b6995ca39ee03ad184")]
public class EtudeBracketTriggerAction : EtudeBracketTrigger, IHashable
{
	public ActionList OnActivated;

	public ActionList OnDeactivated;

	protected override void OnExit()
	{
		OnDeactivated.Run();
	}

	protected override void OnEnter()
	{
		OnActivated.Run();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
