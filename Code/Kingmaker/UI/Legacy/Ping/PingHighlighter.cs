using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Legacy.Ping;

public class PingHighlighter : MonoBehaviour, IPingAction
{
	public enum TypeLit
	{
		Lit,
		Blink
	}

	public bool IsActive;

	public TypeLit Lit;

	public AnimationCurve CurveBlink;

	public Graphic TargetGraphic;

	protected bool m_IsEvaluate;

	protected float m_Alpha;

	protected void OnEnable()
	{
		Reset();
		if (IsActive)
		{
			Highlight(active: true);
		}
		else
		{
			Reset();
		}
	}

	protected void OnDisable()
	{
		Reset();
	}

	public void OnCallbackPingActive(bool active)
	{
		IsActive = active;
		Highlight(active);
	}

	protected void Highlight(bool active)
	{
		if (!IsEvaluate() && base.isActiveAndEnabled)
		{
			StartCoroutine(Evaluate());
		}
		m_IsEvaluate = active;
	}

	protected void Reset()
	{
		m_Alpha = 0f;
		m_IsEvaluate = false;
		SetGraphicAlpha(0f);
	}

	protected bool IsEvaluateLit()
	{
		if (!m_IsEvaluate)
		{
			return !Mathf.Approximately(m_Alpha, 0f);
		}
		return !Mathf.Approximately(m_Alpha, 1f);
	}

	protected bool IsEvaluate()
	{
		if (Lit != TypeLit.Blink)
		{
			return IsEvaluateLit();
		}
		if (!m_IsEvaluate)
		{
			return !Mathf.Approximately(m_Alpha, 0f);
		}
		return true;
	}

	protected IEnumerator Evaluate()
	{
		yield return null;
		while (IsEvaluate() && base.isActiveAndEnabled)
		{
			float num = Time.unscaledDeltaTime * 5f;
			m_Alpha += (m_IsEvaluate ? num : (0f - num));
			m_Alpha = Mathf.Clamp01(m_Alpha);
			float num2 = m_Alpha;
			if (Lit == TypeLit.Blink)
			{
				num2 *= CurveBlink.Evaluate(Time.unscaledTime);
			}
			SetGraphicAlpha(num2);
			yield return null;
		}
	}

	protected void SetGraphicAlpha(float alpha)
	{
		if (!(TargetGraphic == null))
		{
			TargetGraphic.canvasRenderer.SetAlpha(alpha);
		}
	}
}
