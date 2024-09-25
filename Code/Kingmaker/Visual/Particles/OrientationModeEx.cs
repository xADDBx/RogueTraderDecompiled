using UnityEngine;

namespace Kingmaker.Visual.Particles;

public static class OrientationModeEx
{
	public static void Apply(this OrientationModeType mode, Transform transform)
	{
		switch (mode)
		{
		case OrientationModeType.None:
			transform.rotation = Quaternion.identity;
			break;
		case OrientationModeType.Plain:
		{
			float y = transform.rotation.eulerAngles.y;
			transform.rotation = Quaternion.Euler(0f, y, 0f);
			break;
		}
		case OrientationModeType.Full:
			break;
		}
	}
}
