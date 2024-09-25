using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.LocalMap;

public class LocalMapLight : MonoBehaviour
{
	public static LocalMapLight Instance { get; private set; }

	[UsedImplicitly]
	private void OnEnable()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Instance = null;
	}
}
