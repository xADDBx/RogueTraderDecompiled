using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using RogueTrader.Code.ShaderConsts;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Combat;

public class LineOfSightView : ViewBase<LineOfSightVM>
{
	[SerializeField]
	private int m_Density = 3;

	[SerializeField]
	public int m_MinPoint = 10;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_FadeTime = 0.2f;

	[SerializeField]
	private LineRenderer m_LineRenderer;

	[SerializeField]
	private LineOfSightColor[] m_ColorsTable;

	private Gradient m_DefaultGradient;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.StartPos.CombineLatest(base.ViewModel.EndPos, base.ViewModel.BestStartPos, (Vector3 start, Vector3 end, Vector3? bestStartPos) => new { start, end, bestStartPos }).Subscribe(value =>
		{
			SetLine(value.start, value.end, value.bestStartPos);
		}));
		AddDisposable(base.ViewModel.HitChance.Subscribe(delegate(float value)
		{
			m_LineRenderer.colorGradient = GetColorByHitChance(value);
		}));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(m_LineRenderer.gameObject.SetActive));
		m_DefaultGradient = m_LineRenderer.colorGradient;
		FadeInOut(0f, visible: true);
	}

	protected override void DestroyViewImplementation()
	{
		FadeInOut(null, visible: false).OnComplete(delegate
		{
			WidgetFactory.DisposeWidget(this);
		});
	}

	private Gradient GetColorByHitChance(float hitChance)
	{
		LineOfSightColor[] colorsTable = m_ColorsTable;
		foreach (LineOfSightColor lineOfSightColor in colorsTable)
		{
			if (hitChance <= lineOfSightColor.HitChance)
			{
				return lineOfSightColor.Gradient;
			}
		}
		return m_DefaultGradient;
	}

	private Tweener FadeInOut(float? startAlpha, bool visible)
	{
		Color color = m_LineRenderer.material.color;
		color.a = startAlpha ?? color.a;
		m_LineRenderer.material.color = color;
		return m_LineRenderer.material.DOFade(visible ? 1f : 0f, m_FadeTime);
	}

	private void SetLine(Vector3 startPos, Vector3 endPos, Vector3? bestStartPos)
	{
		startPos = bestStartPos ?? startPos;
		startPos += base.ViewModel.StartObjectOffset;
		endPos += base.ViewModel.EndObjectOffset;
		m_LineRenderer.sharedMaterial.SetFloat(ShaderProps._TimeEditor, Time.unscaledTime - Time.time);
		Vector3 vector = new Vector3(0f, 0.1f, 0f);
		startPos += vector;
		endPos += vector;
		float num = Vector3.Distance(startPos, endPos);
		int num2 = Mathf.Max((int)Mathf.Floor((float)m_Density * num), m_MinPoint);
		List<Vector3> list = new List<Vector3>();
		for (float num3 = 0f; num3 < (float)num2; num3 += 1f)
		{
			float t = num3 / (float)num2;
			Vector3 item = Vector3.Lerp(startPos, endPos, t);
			list.Add(item);
		}
		m_LineRenderer.positionCount = num2;
		m_LineRenderer.SetPositions(list.ToArray());
	}
}
