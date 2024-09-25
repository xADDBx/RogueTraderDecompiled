using Kingmaker.Code.UI.MVVM.VM.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class OtherObjectsInfoSpaceSystemInformationWindowView : ViewBase<OtherObjectsInfoSpaceSystemInformationWindowVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private GameObject m_ConsoleFocusButton;

	public MonoBehaviour MonoBehaviour => this;

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((OtherObjectsInfoSpaceSystemInformationWindowVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is OtherObjectsInfoSpaceSystemInformationWindowVM;
	}

	protected override void BindViewImplementation()
	{
		m_Text.text = ((!string.IsNullOrWhiteSpace(base.ViewModel.Name)) ? base.ViewModel.Name : "Empty Name");
		if (base.ViewModel.Icon != null)
		{
			m_Image.sprite = base.ViewModel.Icon;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_ConsoleFocusButton.SetActive(value);
		if (value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	public bool IsValid()
	{
		return base.gameObject.activeSelf;
	}
}
