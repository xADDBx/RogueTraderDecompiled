using UnityEngine;

namespace Kingmaker.View;

internal struct SideResult
{
	public bool Valid;

	public float Angle;

	public bool Sqeeze;

	public SideResult(float angle, bool sqeeze)
	{
		if (Mathf.Abs(angle) > 175f)
		{
			this = Invalid();
			return;
		}
		Valid = true;
		Angle = angle;
		Sqeeze = sqeeze;
	}

	public static SideResult Invalid()
	{
		SideResult result = default(SideResult);
		result.Angle = 1000000f;
		return result;
	}
}
