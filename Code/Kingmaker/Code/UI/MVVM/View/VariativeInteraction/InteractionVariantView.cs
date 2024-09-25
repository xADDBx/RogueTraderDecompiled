using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction;

public class InteractionVariantView : ViewBase<InteractionVariantVM>, IWidgetView
{
	[SerializeField]
	protected OwlcatButton m_Button;

	[Header("Images")]
	[SerializeField]
	private Image m_ImageNormal;

	[SerializeField]
	private Image m_ImageHighlighted;

	[SerializeField]
	private Image m_ImagePressed;

	[SerializeField]
	private Image m_ImageDisabled;

	[Header("Default sprites")]
	[SerializeField]
	private Sprite m_DefaultIconNormal;

	[SerializeField]
	private Sprite m_DefaultIconHighlighted;

	[SerializeField]
	private Sprite m_DefaultIconPressed;

	[SerializeField]
	private Sprite m_DefaultIconDisabled;

	[SerializeField]
	private TextMeshProUGUI m_ActionName;

	[FormerlySerializedAs("m_Value")]
	[SerializeField]
	private TextMeshProUGUI m_ResourceCount;

	[SerializeField]
	private Image m_UnitIcon;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_ImageNormal.sprite = base.ViewModel.ImageNormal ?? m_DefaultIconNormal;
		m_ImageHighlighted.sprite = base.ViewModel.ImageHighlighted ?? m_DefaultIconHighlighted;
		m_ImagePressed.sprite = base.ViewModel.ImagePressed ?? m_DefaultIconPressed;
		m_ImageDisabled.sprite = base.ViewModel.ImageDisabled ?? m_DefaultIconDisabled;
		if (base.ViewModel.RequiredResourceCount.HasValue)
		{
			m_ResourceCount.text = ((base.ViewModel.ResourceCount > 0) ? $"{base.ViewModel.ResourceCount}" : string.Empty);
			m_UnitIcon.gameObject.SetActive(value: false);
		}
		else if (base.ViewModel.OnlyOnceCheck && !base.ViewModel.Disabled)
		{
			m_ResourceCount.text = "1";
			m_UnitIcon.gameObject.SetActive(value: true);
		}
		else if (base.ViewModel.LimitedUnitsCheck)
		{
			m_ResourceCount.text = $"{base.ViewModel.UnitCount}";
			m_UnitIcon.gameObject.SetActive(value: true);
		}
		else
		{
			m_ResourceCount.text = string.Empty;
			m_UnitIcon.gameObject.SetActive(value: false);
		}
		AddDisposable(base.ViewModel.InteractionName.Subscribe(delegate(string text)
		{
			m_ActionName.text = text;
		}));
		m_Button.Interactable = !base.ViewModel.Disabled;
		base.gameObject.name = "InteractionVariantView " + base.ViewModel.InteractionName.Value + " " + base.ViewModel.ResourceName;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as InteractionVariantVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is InteractionVariantVM;
	}
}
