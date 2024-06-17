using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("768bc8481a324c5490c0dd1e63df054f")]
public class EtudeBracketPauseWarpTravel : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.SectorMapTravelController.PauseTravel();
	}

	protected override void OnExit()
	{
		Game.Instance.SectorMapTravelController.UnpauseManual();
	}

	protected override void OnResume()
	{
		Game.Instance.SectorMapTravelController.PauseTravel();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
