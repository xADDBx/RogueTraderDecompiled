using System;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("Layout/Extensions/Radial Layout")]
public class RadialLayout : LayoutGroup
{
	public float fDistance;

	[Range(0f, 360f)]
	public float MinAngle;

	[Range(0f, 360f)]
	public float MaxAngle;

	[Range(0f, 360f)]
	public float StartAngle;

	protected override void OnEnable()
	{
		base.OnEnable();
		CalculateRadial();
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
	}

	public override void CalculateLayoutInputVertical()
	{
		CalculateRadial();
	}

	public override void CalculateLayoutInputHorizontal()
	{
		CalculateRadial();
	}

	private void CalculateRadial()
	{
		m_Tracker.Clear();
		if (base.transform.childCount == 0)
		{
			return;
		}
		float num = (MaxAngle - MinAngle) / (float)base.transform.childCount;
		float num2 = StartAngle;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			RectTransform rectTransform = (RectTransform)base.transform.GetChild(i);
			if (rectTransform != null)
			{
				m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot);
				Vector3 vector = new Vector3(Mathf.Cos(num2 * (MathF.PI / 180f)), Mathf.Sin(num2 * (MathF.PI / 180f)), 0f);
				rectTransform.localPosition = vector * fDistance;
				Vector2 vector3 = (rectTransform.pivot = new Vector2(0.5f, 0.5f));
				Vector2 anchorMin = (rectTransform.anchorMax = vector3);
				rectTransform.anchorMin = anchorMin;
				num2 += num;
			}
		}
	}
}
