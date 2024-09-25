using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationResourceBaseView : ViewBase<ExplorationResourceVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_CountLabel;

	[SerializeField]
	protected OwlcatMultiButton m_FocusButton;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string val)
		{
			m_NameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int val)
		{
			m_CountLabel.text = val.ToString();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ExplorationResourceVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ExplorationResourceVM;
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
		this.ShowTooltip(new TooltipTemplateSimple(base.ViewModel.Name.Value, base.ViewModel.Description.Value));
	}
}
