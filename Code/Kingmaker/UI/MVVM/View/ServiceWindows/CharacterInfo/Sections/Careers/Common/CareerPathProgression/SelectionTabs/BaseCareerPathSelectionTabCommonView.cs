using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;

public abstract class BaseCareerPathSelectionTabCommonView<TViewModel> : ViewBase<TViewModel>, ICareerPathSelectionTabView where TViewModel : class, IViewModel
{
	[Header("Header")]
	[SerializeField]
	protected GameObject m_HeaderBlock;

	[SerializeField]
	protected TextMeshProUGUI m_HeaderLabel;

	protected readonly ReactiveProperty<string> NextButtonLabel = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<string> BackButtonLabel = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<string> FinishButtonLabel = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<bool> IsTabActiveProp = new ReactiveProperty<bool>();

	protected RectTransform RectTransform;

	protected AccessibilityTextHelper TextHelper;

	public virtual void Initialize()
	{
		Hide();
		TextHelper = new AccessibilityTextHelper(m_HeaderLabel);
		RectTransform = GetComponent<RectTransform>();
	}

	protected override void BindViewImplementation()
	{
		TextHelper.UpdateTextSize();
		Show();
	}

	protected override void DestroyViewImplementation()
	{
		TextHelper.Dispose();
		Hide();
	}

	protected void Show()
	{
		base.gameObject.SetActive(value: true);
		IsTabActiveProp.Value = true;
	}

	protected void Hide()
	{
		base.gameObject.SetActive(value: false);
		IsTabActiveProp.Value = false;
	}

	public bool IsTabActive()
	{
		return IsTabActiveProp.Value;
	}

	protected void SetHeader(string text)
	{
		if (m_HeaderBlock != null)
		{
			m_HeaderBlock.gameObject.SetActive(!string.IsNullOrEmpty(text));
		}
		if (m_HeaderLabel != null)
		{
			m_HeaderLabel.text = text;
		}
	}

	protected void SetNextButtonLabel(string text)
	{
		NextButtonLabel.Value = text;
	}

	protected void SetBackButtonLabel(string text)
	{
		BackButtonLabel.Value = text;
	}

	protected void SetFinishButtonLabel(string text)
	{
		FinishButtonLabel.Value = text;
	}

	public virtual void UpdateState()
	{
	}

	protected virtual void HandleClickNext()
	{
	}

	protected virtual void HandleClickBack()
	{
	}

	protected virtual void HandleClickFinish()
	{
	}

	void ICareerPathSelectionTabView.Unbind()
	{
		Unbind();
	}
}
