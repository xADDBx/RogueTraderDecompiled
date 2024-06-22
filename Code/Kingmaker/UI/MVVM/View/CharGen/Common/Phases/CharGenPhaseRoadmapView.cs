using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases;

public class CharGenPhaseRoadmapView<TViewModel> : SelectionGroupEntityView<TViewModel>, ICharGenPhaseRoadmapView, IInitializable where TViewModel : CharGenPhaseBaseVM
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_Separator;

	private CharGenPhaseType m_Type;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	public RectTransform ViewRectTransform => base.transform as RectTransform;

	public void Initialize(CharGenPhaseType type)
	{
		m_Type = type;
		AddDisposable(m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Label));
		ClearState();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		Show();
		AddDisposable(base.ViewModel.PhaseName.Subscribe(delegate(string value)
		{
			m_Label.text = value;
		}));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(delegate
		{
			UpdateSelectableState();
		}));
		AddDisposable(base.ViewModel.IsCompletedAndAvailable.Subscribe(delegate
		{
			UpdateSelectableState();
		}));
		m_Label.text = UIStrings.Instance.CharGen.GetPhaseName(m_Type);
		m_AccessibilityTextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		ClearState();
	}

	protected override void OnClick()
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	public override void OnChangeSelectedState(bool value)
	{
		UpdateSelectableState();
	}

	private void UpdateSelectableState()
	{
		m_Button.Interactable = base.ViewModel.IsAvailable.Value;
		if (base.ViewModel.IsCompletedAndAvailable.Value)
		{
			m_Button.SetActiveLayer(base.ViewModel.IsSelected.Value ? "CompletedSelected" : "Completed");
		}
		else
		{
			m_Button.SetActiveLayer(base.ViewModel.IsSelected.Value ? "Selected" : "Normal");
		}
	}

	private void ClearState()
	{
		m_Button.Interactable = false;
		m_Button.SetActiveLayer("Normal");
		Hide();
	}

	public void SetParentTransform(Transform parent, int siblingIndex = 0)
	{
		base.transform.SetParent(parent, worldPositionStays: false);
		base.transform.SetSiblingIndex(siblingIndex);
	}

	public CharGenPhaseBaseVM GetPhaseBaseVM()
	{
		return base.ViewModel;
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		if (m_Separator != null)
		{
			m_Separator.SetActive(value: true);
		}
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (m_Separator != null)
		{
			m_Separator.SetActive(value: false);
		}
	}

	void IInitializable.Initialize()
	{
		Initialize();
	}
}
