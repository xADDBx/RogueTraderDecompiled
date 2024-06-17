using System;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
public class TrajectoryOffset
{
	public AnimationCurve Curve;

	public float AmplitudeScale = 1f;

	public float FrequencyScale = 1f;

	public float ScrollSpeed;

	public float RandomOffset;

	private float m_OnInitializeStaticOffset;

	public float OnInitializeStaticOffset
	{
		get
		{
			return m_OnInitializeStaticOffset;
		}
		set
		{
			m_OnInitializeStaticOffset = value;
		}
	}
}
