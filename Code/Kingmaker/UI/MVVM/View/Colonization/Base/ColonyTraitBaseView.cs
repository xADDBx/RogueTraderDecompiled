using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Traits;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyTraitBaseView : ViewBase<ColonyTraitVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[FormerlySerializedAs("m_Name")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected OwlcatMultiButton m_FocusButton;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string val)
		{
			m_NameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.Icon.Subscribe(SetIcon));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetIcon(Sprite icon)
	{
		if (!(icon == null))
		{
			m_Icon.sprite = icon;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ColonyTraitVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyTraitVM;
	}

	public void SetFocus(bool value)
	{
		m_FocusButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public bool CanInteract()
	{
		return false;
	}

	public bool CanShowTooltip()
	{
		return true;
	}

	public void Interact()
	{
	}

	public void ShowTooltip()
	{
		this.ShowTooltip(base.ViewModel.Tooltip.Value);
	}
}
