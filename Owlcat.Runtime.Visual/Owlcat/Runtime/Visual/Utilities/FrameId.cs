using UnityEngine;

namespace Owlcat.Runtime.Visual.Utilities;

public static class FrameId
{
	private static int s_FrameCount;

	private static float s_Time;

	private static float s_LastTime;

	public static int FrameCount => s_FrameCount;

	internal static void Update()
	{
		float num;
		bool flag;
		if (Application.isPlaying)
		{
			num = Time.time;
			int frameCount = Time.frameCount;
			flag = s_FrameCount != frameCount;
		}
		else
		{
			num = Time.realtimeSinceStartup;
			flag = num - s_Time > 0.0166f || num <= s_Time;
		}
		if (flag)
		{
			if (num >= s_Time)
			{
				s_FrameCount++;
			}
			else
			{
				s_FrameCount = 0;
			}
			s_LastTime = ((num > s_Time) ? s_Time : 0f);
			s_Time = num;
		}
	}
}
