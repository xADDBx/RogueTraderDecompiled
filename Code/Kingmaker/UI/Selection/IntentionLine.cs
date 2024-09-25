using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.UI.Selection;

[RequireComponent(typeof(LineRenderer))]
public class IntentionLine : MonoBehaviour
{
	public AnimationCurve Curve;

	public int Density = 3;

	public int MinPoint = 10;

	[Range(0f, 2f)]
	public float FadeTime = 0.2f;

	private LineRenderer m_LineRenderer;

	private Color2 m_StartColor;

	private Color2 m_EndColor;

	private bool m_IsActive;

	private Vector3 m_EndPos;

	private Vector3 m_StartPos;

	private BaseUnitEntity m_EndUnit;

	private BaseUnitEntity m_StartUnit;

	private Vector3 m_OverrideEndPosition;

	private Vector3 m_OverrideStartPosition;

	private LineRenderer Line => m_LineRenderer = (m_LineRenderer ? m_LineRenderer : GetComponent<LineRenderer>());

	public void Initialize()
	{
		m_EndColor = new Color2(Line.startColor, Line.endColor);
		Color startColor = Line.startColor;
		Color endColor = Line.endColor;
		m_StartColor = new Color2(startColor, endColor);
		Line.startColor = startColor;
		Line.endColor = endColor;
		Line.gameObject.SetActive(value: false);
	}

	public void Show(BaseUnitEntity startUnit, Vector3 position)
	{
		m_OverrideEndPosition = position;
		Show(startUnit, null);
	}

	public void Show(Vector3 startPosition, Vector3 endPosition)
	{
		m_OverrideStartPosition = startPosition;
		m_OverrideEndPosition = endPosition;
		Show(null, null);
	}

	public void Show(Vector3 startPosition, BaseUnitEntity endUnit)
	{
		m_OverrideStartPosition = startPosition;
		Show(null, endUnit);
	}

	public void Show(BaseUnitEntity startUnit, BaseUnitEntity endUnit)
	{
		if (!m_IsActive || startUnit != m_StartUnit || m_EndUnit != endUnit)
		{
			m_StartUnit = startUnit;
			m_EndUnit = endUnit;
			SetLine();
			m_IsActive = true;
			Line.gameObject.SetActive(value: true);
			Color2 endValue = m_EndColor;
			if (startUnit != null && startUnit.Faction.IsPlayer && !UIAccess.SelectionManager.IsSelected(startUnit))
			{
				Color startColor = Line.startColor;
				startColor.a = 0f;
				Color endColor = Line.endColor;
				endColor.a = 0.5f;
				endValue = new Color2(startColor, endColor);
			}
			Line.DOColor(new Color2(Line.startColor, Line.endColor), endValue, FadeTime).SetUpdate(isIndependentUpdate: true);
		}
	}

	private void SetLine()
	{
		Line.sharedMaterial.SetFloat(ShaderProps._TimeEditor, Time.unscaledTime - Time.time);
		Vector3 vector = Vector3.up * 0.5f;
		m_StartPos = ((m_StartUnit != null) ? (m_StartUnit.Position + vector) : m_OverrideStartPosition);
		m_EndPos = ((m_EndUnit != null) ? (m_EndUnit.Position + vector) : m_OverrideEndPosition);
		float num = Vector3.Distance(m_StartPos, m_EndPos);
		int num2 = Mathf.Max((int)Mathf.Floor((float)Density * num), MinPoint);
		Vector3 vector2 = Vector3.up;
		if (IsUnitsAreAttackEachOther())
		{
			vector2 = (m_StartUnit.Faction.IsPlayer ? (vector2 + Vector3.left / 50f).normalized : (vector2 + Vector3.right / 50f).normalized);
		}
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < num2; i++)
		{
			float num3 = (float)i / (float)num2;
			Vector3 item = Vector3.Lerp(m_StartPos, m_EndPos, num3);
			item += vector2 * Curve.Evaluate(num3);
			list.Add(item);
		}
		Line.positionCount = num2;
		Line.SetPositions(list.ToArray());
	}

	private bool IsUnitsAreAttackEachOther()
	{
		if (m_EndUnit == null || m_StartUnit == null)
		{
			return false;
		}
		if (m_EndUnit.Commands.Current is UnitUseAbility unitUseAbility && unitUseAbility.TargetUnit == m_StartUnit)
		{
			return true;
		}
		return false;
	}

	[UsedImplicitly]
	private void Update()
	{
		if (m_IsActive)
		{
			if ((m_StartUnit != null && m_StartUnit.LifeState.IsFinallyDead) || (m_EndUnit != null && m_EndUnit.LifeState.IsFinallyDead))
			{
				Hide();
			}
			if ((m_StartUnit != null && !m_StartUnit.IsVisibleForPlayer) || (m_EndUnit != null && !m_EndUnit.IsVisibleForPlayer))
			{
				Hide();
			}
			if (m_StartPos != (m_StartUnit?.Position ?? m_OverrideStartPosition) || m_EndPos != (m_EndUnit?.Position ?? m_OverrideEndPosition))
			{
				SetLine();
			}
		}
	}

	public void Hide()
	{
		if (m_IsActive)
		{
			m_IsActive = false;
			Line.DOColor(new Color2(Line.startColor, Line.endColor), m_StartColor, FadeTime).SetUpdate(isIndependentUpdate: true).OnComplete(OnCompleteHide);
		}
	}

	private void OnCompleteHide()
	{
		if (!m_IsActive)
		{
			Line.gameObject.SetActive(value: false);
		}
	}
}
