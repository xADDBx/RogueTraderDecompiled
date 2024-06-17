using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("ce59c67ae1d44a6785beebe6a08b72d1")]
public class EtudeBracketLockTransferFromCargo : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.Player.CargoState.LockTransferFromCargo.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.Player.CargoState.LockTransferFromCargo.Release();
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
