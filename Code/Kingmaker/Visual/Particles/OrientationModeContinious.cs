using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class OrientationModeContinious : MonoBehaviour
{
	public OrientationModeType Mode;

	public void Update()
	{
		Mode.Apply(base.gameObject.transform);
	}
}
