using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.CombatText;

public class OvertipCombatTextBlockView : ViewBase<OvertipCombatTextBlockVM>
{
	[SerializeField]
	private Color DefaultColor = Color.white;

	[Header("Common Combat Texts")]
	[SerializeField]
	private FadeAnimator m_CombatTextContainerAnimator;

	[SerializeField]
	private CombatTextCommonCreator m_CombatTextCommonCreator;

	[Header("Common Combat Texts: Variable position")]
	[SerializeField]
	private RectTransform m_ScaledRect;

	[SerializeField]
	private List<RectTransform> m_PartRects;

	private List<CanvasGroup> m_PartCanvasGroups;

	[SerializeField]
	private float m_BottomPadding = 5f;

	[Header("Hit points Combat Texts")]
	[SerializeField]
	private CombatTextHitPointsCreator m_CombatTextHitPointsCreator;

	[HideInInspector]
	public BoolReactiveProperty HasActiveCombatText = new BoolReactiveProperty();

	private bool m_IsInitialize;

	[SerializeField]
	private float m_MoveTime = 0.1f;

	private readonly ReactiveCommand m_UpdateVisual = new ReactiveCommand();

	private List<CombatMessageBase> m_MessagesList = new List<CombatMessageBase>();

	private bool m_IsActive;

	protected override void BindViewImplementation()
	{
		AddDisposable(UniRxExtensionMethods.Subscribe(m_UpdateVisual.ObserveLastValueOnLateUpdate(), delegate
		{
			UpdateVisualInternal();
		}));
		m_CombatTextCommonCreator.SetCallback(delegate
		{
			m_UpdateVisual.Execute();
		});
		m_CombatTextCommonCreator.Initialize(base.ViewModel.MechanicEntity);
		m_PartCanvasGroups = m_PartRects.Select((RectTransform r) => r.GetComponent<CanvasGroup>()).ToList();
		AddDisposable(base.ViewModel.CombatMessage.Subscribe(OnCombatMessage));
		m_CombatTextCommonCreator.Clear();
		m_CombatTextHitPointsCreator.Clear();
		m_UpdateVisual.Execute();
	}

	protected override void DestroyViewImplementation()
	{
		m_CombatTextCommonCreator.Dispose();
		m_CombatTextCommonCreator.Clear();
		m_CombatTextHitPointsCreator.Clear();
		UpdateIsCombatText();
	}

	private void OnCombatMessage(CombatMessageBase message)
	{
		if (!base.ViewModel.IsCutscene)
		{
			m_MessagesList.Add(message);
			DelayedInvoker.InvokeInTime(AddCombatMessage, 0.1f);
		}
	}

	private void AddCombatMessage()
	{
		if (m_MessagesList.Count == 1)
		{
			AddCombatText(m_MessagesList.First(), single: true);
		}
		else
		{
			for (int i = 0; i < m_MessagesList.Count; i++)
			{
				AddCombatText(m_MessagesList[i], single: false, i % 2 > 0);
			}
		}
		m_MessagesList.Clear();
		m_UpdateVisual.Execute();
	}

	private void UpdateVisualInternal()
	{
		UpdateIsCombatText();
		if (HasActiveCombatText.Value && !m_IsActive)
		{
			m_CombatTextContainerAnimator.AppearAnimation();
			m_IsActive = true;
		}
		else if (!HasActiveCombatText.Value && m_IsActive)
		{
			m_CombatTextContainerAnimator.DisappearAnimation();
			m_IsActive = false;
			m_CombatTextCommonCreator.ContainerRect.sizeDelta = Vector2.zero;
			return;
		}
		if (!m_PartRects.Any())
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < m_PartRects.Count; i++)
		{
			RectTransform rectTransform = m_PartRects[i];
			if (m_PartCanvasGroups[i].alpha > 0f && rectTransform.gameObject.activeInHierarchy)
			{
				num = Mathf.Max(RectTransformUtility.CalculateRelativeRectTransformBounds(m_CombatTextCommonCreator.ContainerRect.parent.transform, rectTransform).max.y, num);
			}
		}
		m_CombatTextCommonCreator.ContainerRect.anchoredPosition = new Vector2(m_CombatTextCommonCreator.ContainerRect.anchoredPosition.x, num + m_BottomPadding);
		Vector2 sizeDelta = default(Vector2);
		for (int j = 0; j < m_CombatTextCommonCreator.ActiveViews.Count; j++)
		{
			CombatTextEntityBaseView<CombatMessageBase> combatTextEntityBaseView = m_CombatTextCommonCreator.ActiveViews[j];
			sizeDelta = new Vector2(Mathf.Max(sizeDelta.x, combatTextEntityBaseView.Size.x), sizeDelta.y + combatTextEntityBaseView.Size.y);
			combatTextEntityBaseView.Rect.anchoredPosition = new Vector2(combatTextEntityBaseView.XPos, (float)j * combatTextEntityBaseView.Rect.rect.height);
		}
		m_CombatTextCommonCreator.ContainerRect.sizeDelta = sizeDelta;
	}

	private void AddCombatText(CombatMessageBase message, bool single, bool even = true)
	{
		if (!(message is CombatMessageDamage combatMessageDamage))
		{
			if (!(message is CombatMessageAttackMiss combatMessageAttackMiss))
			{
				if (message is CombatMessageHealing)
				{
					CombatTextHitPointsView combatTextHitPointsView = m_CombatTextHitPointsCreator.Create(message);
					combatTextHitPointsView.SetDirection(UIUtilityGetRect.GetObjectPositionInCamera(Vector3.zero) - UIUtilityGetRect.GetObjectPositionInCamera(Vector3.zero), single: false, even: false);
					combatTextHitPointsView.PlayAnimation();
				}
				else
				{
					m_CombatTextCommonCreator.Create(message);
					UpdateIsCombatText();
				}
			}
			else if (combatMessageAttackMiss.Result == AttackResult.Miss)
			{
				m_CombatTextHitPointsCreator.Create(message).PlayAnimation();
			}
			else
			{
				m_CombatTextCommonCreator.Create(message);
			}
		}
		else
		{
			CombatTextHitPointsView combatTextHitPointsView2 = m_CombatTextHitPointsCreator.Create(message);
			combatTextHitPointsView2.SetDirection(UIUtilityGetRect.GetObjectPositionInCamera(combatMessageDamage.TargetPosition) - UIUtilityGetRect.GetObjectPositionInCamera(combatMessageDamage.SourcePosition), single, even);
			combatTextHitPointsView2.PlayAnimation();
		}
	}

	private void UpdateIsCombatText()
	{
		HasActiveCombatText.Value = m_CombatTextCommonCreator.ActiveViews.Any();
	}

	public void UpdateVisualForCommon()
	{
		m_UpdateVisual.Execute();
	}
}
