using UnityEngine;

namespace Kingmaker.QA.Arbiter.Service;

public class GameCoreSceneBoundary : ISceneBoundary
{
	public Bounds Get()
	{
		return Game.Instance.CurrentlyLoadedArea.Bounds.CameraBounds;
	}
}
