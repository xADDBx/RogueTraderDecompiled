using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
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
		}
		else
		{
			m_ResourceCount.text = string.Empty;
		}
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			AddDisposable(this.SetHint(GetHint()));
		}
		AddDisposable(base.ViewModel.InteractionName.Subscribe(delegate(string text)
		{
			m_ActionName.text = text;
		}));
		m_Button.Interactable = !base.ViewModel.Disabled;
	}

	protected string GetHint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.ViewModel.ResourceName + "\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.HasResourceCount.Text}: {base.ViewModel.ResourceCount}\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.RequiredResourceCount.Text}: {base.ViewModel.RequiredResourceCount}\n");
		return stringBuilder.ToString();
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
