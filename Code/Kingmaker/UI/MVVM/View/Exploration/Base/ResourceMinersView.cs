using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ResourceMinersView : ViewBase<ResourceMinersVM>, IWidgetView, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IExplorationComponentEntity
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		m_NameLabel.text = UIStrings.Instance.ExplorationTexts.ResourceMiner.Text;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int value)
		{
			m_Count.text = $"x{value}";
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ResourceMinersVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ResourceMinersVM;
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.isActiveAndEnabled)
		{
			ResourceMinersVM viewModel = base.ViewModel;
			if (viewModel == null)
			{
				return false;
			}
			return !viewModel.HasColony.Value;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
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
