using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual;

public class CameraShakeFx : MonoBehaviour
{
	public AnimationCurve DeltaX;

	public AnimationCurve DeltaY;

	[Space(10f)]
	public AnimationCurve AmplitudeOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve FreqOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[Space(10f)]
	public AnimationCurve AmplitudeOverDistance = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve FreqOverDistance = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[Space(10f)]
	public bool ShakeAnchor;

	public float AmplitudeMultiplier = 1f;

	public float FreqMultiplier = 1f;

	public bool InvertXRandomly;

	public bool InvertYRandomly;

	[Space(10f)]
	public float Delay;

	private float m_Time;

	private float m_Point;

	private bool m_InvertX;

	private bool m_InvertY;

	public void OnEnable()
	{
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			m_Time = 0f;
			m_Point = 0f;
			m_InvertX = InvertXRandomly && Random.value < 0.5f;
			m_InvertY = InvertYRandomly && Random.value < 0.5f;
			instance.AddShakeFx(this);
		}
	}

	public void OnDisable()
	{
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			instance.RemoveShakeFx(this);
		}
	}

	public Vector2 CalculateDelta(Vector3 camPos)
	{
		float time = Vector3.Distance(camPos, base.transform.position);
		float num = FreqOverLifetime.Evaluate(m_Time) * FreqOverDistance.Evaluate(time) * FreqMultiplier;
		m_Time += Time.deltaTime;
		if (m_Time < Delay)
		{
			return Vector2.zero;
		}
		m_Point += num * Time.deltaTime;
		float num2 = AmplitudeOverLifetime.Evaluate(m_Time) * AmplitudeOverDistance.Evaluate(time) * AmplitudeMultiplier;
		float x = DeltaX.Evaluate(m_Point) * num2 * (float)((!m_InvertX) ? 1 : (-1));
		float y = DeltaY.Evaluate(m_Point) * num2 * (float)((!m_InvertY) ? 1 : (-1));
		return new Vector2(x, y);
	}
}
