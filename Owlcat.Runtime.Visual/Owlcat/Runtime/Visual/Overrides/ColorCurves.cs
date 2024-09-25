using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Color Curves")]
public sealed class ColorCurves : VolumeComponent, IPostProcessComponent
{
	public TextureCurveParameter master;

	public TextureCurveParameter red;

	public TextureCurveParameter green;

	public TextureCurveParameter blue;

	public TextureCurveParameter hueVsHue;

	public TextureCurveParameter hueVsSat;

	public TextureCurveParameter satVsSat;

	public TextureCurveParameter lumVsSat;

	public bool IsActive()
	{
		return true;
	}

	public bool IsTileCompatible()
	{
		return true;
	}

	public ColorCurves()
	{
		Keyframe[] keys = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		Vector2 bounds = new Vector2(0f, 1f);
		master = new TextureCurveParameter(new TextureCurve(keys, 0f, loop: false, in bounds));
		Keyframe[] keys2 = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		bounds = new Vector2(0f, 1f);
		red = new TextureCurveParameter(new TextureCurve(keys2, 0f, loop: false, in bounds));
		Keyframe[] keys3 = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		bounds = new Vector2(0f, 1f);
		green = new TextureCurveParameter(new TextureCurve(keys3, 0f, loop: false, in bounds));
		Keyframe[] keys4 = new Keyframe[2]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		};
		bounds = new Vector2(0f, 1f);
		blue = new TextureCurveParameter(new TextureCurve(keys4, 0f, loop: false, in bounds));
		Keyframe[] keys5 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		hueVsHue = new TextureCurveParameter(new TextureCurve(keys5, 0.5f, loop: true, in bounds));
		Keyframe[] keys6 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		hueVsSat = new TextureCurveParameter(new TextureCurve(keys6, 0.5f, loop: true, in bounds));
		Keyframe[] keys7 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		satVsSat = new TextureCurveParameter(new TextureCurve(keys7, 0.5f, loop: false, in bounds));
		Keyframe[] keys8 = new Keyframe[0];
		bounds = new Vector2(0f, 1f);
		lumVsSat = new TextureCurveParameter(new TextureCurve(keys8, 0.5f, loop: false, in bounds));
		base._002Ector();
	}
}
