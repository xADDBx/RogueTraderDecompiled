using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("f905ab8b4c014637b79eae5da34c6fd5")]
public class EtudeIgnorePersonalEncumbrance : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePersonalEncumbrance.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePersonalEncumbrance.Release();
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
