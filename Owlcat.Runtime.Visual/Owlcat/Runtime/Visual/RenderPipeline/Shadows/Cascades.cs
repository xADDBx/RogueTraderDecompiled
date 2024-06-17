using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows;

[Serializable]
public class Cascades
{
	[Range(128f, 4096f)]
	public int Resolution = 1024;

	[Range(1f, 4f)]
	public int Count = 1;

	public float Cascade2Splits = 0.067f;

	public Vector2 Cascade3Splits = new Vector2(0.067f, 0.2f);

	public Vector3 Cascade4Splits = new Vector3(0.067f, 0.2f, 0.467f);

	public Vector3 GetRatios()
	{
		return Count switch
		{
			2 => new Vector3(Cascade2Splits, 0f), 
			3 => Cascade3Splits, 
			4 => Cascade4Splits, 
			_ => new Vector3(1f, 0f, 0f), 
		};
	}
}
