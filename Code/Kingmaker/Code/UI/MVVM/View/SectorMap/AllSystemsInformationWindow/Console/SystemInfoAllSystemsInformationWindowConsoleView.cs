using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Base;
using Kingmaker.Globalmap.SectorMap;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Console;

public class SystemInfoAllSystemsInformationWindowConsoleView : SystemInfoAllSystemsInformationWindowBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private GameObject m_ConsoleFocusButton;

	[SerializeField]
	private ConsoleHint m_ConsoleAcceptHint;

	public BoolReactiveProperty IsShouldBeFocusedAnyway = new BoolReactiveProperty();

	public BoolReactiveProperty IsFocused { get; } = new BoolReactiveProperty();


	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ConsoleAcceptHint.BindCustomAction(8, GamePad.Instance.CurrentInputLayer, IsFocused));
		m_ConsoleAcceptHint.SetLabel(UIStrings.Instance.GlobalMap.SystemInfo);
	}

	public void ShowFocusBorder(bool state)
	{
		IsShouldBeFocusedAnyway.Value = state;
		m_ConsoleFocusButton.SetActive(state);
	}

	public void SetFocus(bool value)
	{
		IsFocused.Value = value;
		m_ConsoleFocusButton.SetActive(IsShouldBeFocusedAnyway.Value || value);
	}

	public bool IsValid()
	{
		return base.gameObject.activeSelf;
	}

	public SectorMapObjectEntity GetCurrentObjectEntity()
	{
		return base.ViewModel.SectorMapObject;
	}
}
