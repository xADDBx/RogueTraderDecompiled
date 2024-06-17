using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Kingmaker.UI.Selection;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{
	public AnimationCurve Curve;

	public int Density = 3;

	public int MinPoint = 10;

	[Range(0f, 2f)]
	public float FadeTime = 0.2f;

	public Transform FirstPoint;

	public Transform LastPoint;

	private LineRenderer m_LineRenderer;

	private Color2 m_StartColor;

	private Color2 m_EndColor;

	private Vector3 m_EndPos;

	private Vector3 m_StartPos;

	private LineRenderer Line => m_LineRenderer = (m_LineRenderer ? m_LineRenderer : GetComponent<LineRenderer>());

	public void Start()
	{
		m_EndColor = new Color2(Line.startColor, Line.endColor);
		Color startColor = Line.startColor;
		startColor.a = 0f;
		Color endColor = Line.endColor;
		endColor.a = 0f;
		m_StartColor = new Color2(startColor, endColor);
		Line.startColor = startColor;
		Line.endColor = endColor;
		SetLine();
		Color2 endColor2 = m_EndColor;
		Line.DOColor(new Color2(Line.startColor, Line.endColor), endColor2, FadeTime).SetUpdate(isIndependentUpdate: true);
	}

	private void SetLine()
	{
		m_StartPos = FirstPoint.position;
		m_EndPos = LastPoint.position;
		float num = Vector3.Distance(m_StartPos, m_EndPos);
		int num2 = Mathf.Max((int)Mathf.Floor((float)Density * num), MinPoint);
		Vector3 up = Vector3.up;
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < num2; i++)
		{
			float num3 = (float)i / (float)num2;
			Vector3 item = Vector3.Lerp(m_StartPos, m_EndPos, num3);
			item += up * Curve.Evaluate(num3);
			list.Add(item);
		}
		Line.positionCount = num2;
		Line.SetPositions(list.ToArray());
	}
}
