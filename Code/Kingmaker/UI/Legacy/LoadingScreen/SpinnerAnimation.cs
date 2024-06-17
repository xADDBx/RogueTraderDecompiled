using UnityEngine;

namespace Kingmaker.UI.Legacy.LoadingScreen;

public class SpinnerAnimation : MonoBehaviour
{
	public float RotSpeed = 360f;

	public float ScaleSpeed = 1f;

	public float ScalePeriod = 1f;

	public AnimationCurve ScaleCurve;

	private void Update()
	{
		base.transform.Rotate(0f, 0f, Time.unscaledDeltaTime * RotSpeed);
		base.transform.localScale = ScaleCurve.Evaluate(Mathf.Repeat(Time.unscaledTime * ScaleSpeed, ScalePeriod)) * Vector3.one;
	}
}
