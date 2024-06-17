using UnityEngine;

public class CustomRangeAttribute : PropertyAttribute
{
	public float min;

	public float max;

	public CustomRangeAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
