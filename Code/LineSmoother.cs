using System.Collections.Generic;
using UnityEngine;

public class LineSmoother : MonoBehaviour
{
	public static Vector3[] SmoothLine(Vector3[] inputPoints, float segmentSize)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		AnimationCurve animationCurve2 = new AnimationCurve();
		AnimationCurve animationCurve3 = new AnimationCurve();
		Keyframe[] array = new Keyframe[inputPoints.Length];
		Keyframe[] array2 = new Keyframe[inputPoints.Length];
		Keyframe[] array3 = new Keyframe[inputPoints.Length];
		for (int i = 0; i < inputPoints.Length; i++)
		{
			array[i] = new Keyframe(i, inputPoints[i].x);
			array2[i] = new Keyframe(i, inputPoints[i].y);
			array3[i] = new Keyframe(i, inputPoints[i].z);
		}
		animationCurve.keys = array;
		animationCurve2.keys = array2;
		animationCurve3.keys = array3;
		for (int j = 0; j < inputPoints.Length; j++)
		{
			animationCurve.SmoothTangents(j, 0f);
			animationCurve2.SmoothTangents(j, 0f);
			animationCurve3.SmoothTangents(j, 0f);
		}
		List<Vector3> list = new List<Vector3>();
		for (int k = 0; k < inputPoints.Length; k++)
		{
			list.Add(inputPoints[k]);
			if (k + 1 < inputPoints.Length)
			{
				int num = (int)(Vector3.Distance(inputPoints[k], inputPoints[k + 1]) / segmentSize);
				for (int l = 1; l < num; l++)
				{
					float time = (float)l / (float)num + (float)k;
					Vector3 item = new Vector3(animationCurve.Evaluate(time), animationCurve2.Evaluate(time), animationCurve3.Evaluate(time));
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}
}
