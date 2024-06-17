using UnityEngine;

namespace Kingmaker;

public class AnimationDurationAttribute : PropertyAttribute
{
	public readonly int FrameRate;

	public AnimationDurationAttribute(int frameRate = 30)
	{
		FrameRate = frameRate;
	}
}
