using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("8b25726bac4999347b6946ed9ade440e")]
public class EtudePeacefulZone : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.Peaceful.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.Peaceful.Release();
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
