using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("44bf98d9936c43b092cdfec07c997418")]
public class EtudeBracketForbidJumpToWarp : EtudeBracketTrigger, IHashable
{
	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.CannotJumpToWarp.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.CannotJumpToWarp.Release();
	}

	protected override void OnResume()
	{
		Game.Instance.LoadedAreaState.Settings.CannotJumpToWarp.Retain();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
