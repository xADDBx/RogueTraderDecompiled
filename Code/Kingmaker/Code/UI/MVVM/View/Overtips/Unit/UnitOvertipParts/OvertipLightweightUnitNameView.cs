using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipLightweightUnitNameView : ViewBase<OvertipNameBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_NameText;

	private CanvasRenderer m_CharacterNameCanvasRenderer;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private List<RectTransform> m_ContainersList;

	private CanvasRenderer CharacterNameCanvasRenderer => m_CharacterNameCanvasRenderer ?? (m_CharacterNameCanvasRenderer = m_NameText.GetComponent<CanvasRenderer>());

	private bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.UnitState.Unit.IsVisibleForPlayer && !base.ViewModel.UnitState.HasHiddenCondition.Value)
			{
				return base.ViewModel.UnitState.IsMouseOverUnit.Value;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UnitState.IsVisibleForPlayer.CombineLatest(base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.HasHiddenCondition, base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead, base.ViewModel.UnitState.HoverSelfTargetAbility, (bool _, bool _, bool _, bool _, bool _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		AddDisposable(base.ViewModel.UnitState.Name.Subscribe(SetName));
		if (m_MultiSelectable != null)
		{
			AddDisposable(base.ViewModel.UnitState.IsEnemy.Subscribe(delegate(bool value)
			{
				m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.UnitState.IsPlayer.Value ? "Party" : "Ally"));
			}));
		}
		AddDisposable(m_NameText.SetHint(base.ViewModel.UnitState.Name, null, CharacterNameCanvasRenderer.GetColor()));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetName(string value)
	{
		((RectTransform)m_NameText.transform).sizeDelta = new Vector2(1000f, ((RectTransform)m_NameText.transform).sizeDelta.y);
		m_NameText.text = value;
		m_NameText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		((RectTransform)m_NameText.transform).sizeDelta = new Vector2(m_NameText.renderedWidth, ((RectTransform)m_NameText.transform).sizeDelta.y);
		foreach (RectTransform containers in m_ContainersList)
		{
			containers.sizeDelta = new Vector2(m_NameText.renderedWidth, containers.sizeDelta.y);
		}
	}

	private void UpdateVisibility()
	{
		m_Animator.PlayAnimation(CheckVisibility);
	}
}
