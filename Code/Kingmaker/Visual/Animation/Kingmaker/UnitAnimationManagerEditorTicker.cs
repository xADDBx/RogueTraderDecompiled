using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class UnitAnimationManagerEditorTicker : MonoBehaviour
{
	private void Update()
	{
		Object.FindObjectsOfType<UnitAnimationManager>().ForEach(delegate(UnitAnimationManager m)
		{
			m.Tick(Time.deltaTime);
		});
	}
}
