using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarPartWeaponsConsoleView : ViewBase<SurfaceActionBarPartWeaponsVM>
{
	[SerializeField]
	private SurfaceActionBarWeaponSetConsoleView m_CurrentWeaponSet;

	[SerializeField]
	private ConsoleHint m_ChangeSetSurfaceHint;

	[SerializeField]
	private ConsoleHint m_ChangeSetCombatHint;

	[SerializeField]
	private ConsoleHint m_ChangeSetQuickAccessHint;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentSet.Subscribe(m_CurrentWeaponSet.Bind));
	}

	private void ChangeWeaponSet(InputActionEventData data)
	{
		UISounds.Instance.Sounds.ActionBar.WeaponListOpen.Play();
		base.ViewModel.ChangeWeaponSet();
	}

	public void AddInput(InputLayer inputLayer)
	{
		ConsoleHint consoleHint;
		InputActionEventType eventType;
		switch (inputLayer.ContextName)
		{
		case "SurfaceMainInputLayer":
			consoleHint = m_ChangeSetSurfaceHint;
			eventType = InputActionEventType.ButtonJustLongPressed;
			break;
		case "SurfaceCombatInputLayer":
			consoleHint = m_ChangeSetCombatHint;
			eventType = InputActionEventType.ButtonJustLongPressed;
			break;
		case "SurfaceActionBarPartQuickAccessConsoleView":
			consoleHint = m_ChangeSetQuickAccessHint;
			eventType = InputActionEventType.ButtonJustReleased;
			break;
		default:
			consoleHint = m_ChangeSetSurfaceHint;
			eventType = InputActionEventType.ButtonJustLongPressed;
			break;
		}
		AddDisposable(consoleHint.Bind(inputLayer.AddButton(ChangeWeaponSet, 10, base.ViewModel.CanSwitchSets, eventType)));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
