using System;
using Code.GameCore.Modding;
using Code.Utility.ExtendedModInfo;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;

public class DlcManagerModEntityVM : SelectionGroupEntityVM
{
	private bool OldState;

	public readonly BoolReactiveProperty ModSwitchState = new BoolReactiveProperty();

	public readonly BoolReactiveProperty WarningUpdateMod = new BoolReactiveProperty();

	public readonly BoolReactiveProperty WarningReloadGame = new BoolReactiveProperty();

	public readonly bool IsSaveAllowed;

	private readonly ReactiveCommand<bool> m_CheckModNeedToReloadCommand;

	public readonly BoolReactiveProperty ModSettingsAvailable = new BoolReactiveProperty();

	public readonly ExtendedModInfo ModInfo;

	public DlcManagerModEntityVM(ExtendedModInfo modInfo, bool isMainMenu, ReactiveCommand<bool> checkModNeedToReloadCommand)
		: base(allowSwitchOff: false)
	{
		ModInfo = modInfo;
		m_CheckModNeedToReloadCommand = checkModNeedToReloadCommand;
		ModSettingsAvailable.Value = modInfo.HasSettings;
		WarningUpdateMod.Value = modInfo.UpdateRequired;
		OldState = GetActualModState();
		SetTempModState(OldState);
		IsSaveAllowed = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual, isMainMenu);
	}

	protected override void DisposeImplementation()
	{
	}

	public void ShowDescription(bool state)
	{
		if (state)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(null, ModInfo.DisplayName + Environment.NewLine + ModInfo.Author + " - " + ModInfo.Version, ModInfo.Description ?? "");
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleHideSettingsDescription();
			});
		}
	}

	private bool GetActualModState()
	{
		return ModInfo.Enabled;
	}

	public void SetActualModState()
	{
		ModInitializer.EnableMod(ModInfo.Id, ModSwitchState.Value, forceUpdate: true);
	}

	public void ResetTempModState()
	{
		SetTempModState(OldState);
	}

	public void ChangeValue()
	{
		if (IsSaveAllowed)
		{
			SetTempModState(!ModSwitchState.Value);
			SetActualModState();
		}
		else
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.DlcManager.CannotChangeModSwitchState, addToLog: true, WarningNotificationFormat.Attention);
			});
		}
	}

	private void SetTempModState(bool state)
	{
		ModSwitchState.Value = state;
		bool flag = state != OldState;
		WarningReloadGame.Value = flag;
		m_CheckModNeedToReloadCommand.Execute(flag);
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
	}

	public void OpenModSettings()
	{
		ModInitializer.OpenModInfoWindow(ModInfo.Id);
	}
}
