using UnityEngine;

namespace Kingmaker.Sound;

public class AkSetup : IAkCallbacks
{
	public void SetupGameObject(GameObject go)
	{
		if (!go.TryGetComponent<AudioObject>(out var _))
		{
			go.AddComponent<AudioObject>();
		}
	}
}
