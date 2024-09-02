using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.SwitchOnDlcs;

public class DlcManagerSwitchOnDlcEntityVM : SelectionGroupEntityVM
{
	public readonly string Title;

	public readonly BlueprintDlc BlueprintDlc;

	public readonly BoolReactiveProperty DlcSwitchState = new BoolReactiveProperty();

	public readonly BoolReactiveProperty WarningResaveGame = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NeedSwitchOnWarning = new BoolReactiveProperty();

	public readonly bool IsSaveAllowed;

	private readonly ReactiveCommand<bool> m_CheckModNeedToResaveCommand;

	public readonly bool ItIsLateToSwitchDlcOn;

	public readonly string ToLateReason;

	public DlcManagerSwitchOnDlcEntityVM(BlueprintDlc blueprintDlc, ReactiveCommand<bool> checkModNeedToResaveCommand)
		: base(allowSwitchOff: false)
	{
		BlueprintDlc = blueprintDlc;
		Title = blueprintDlc.GetDlcName();
		m_CheckModNeedToResaveCommand = checkModNeedToResaveCommand;
		SetTempDlcState(GetActualDlcState());
		IsSaveAllowed = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual) && !Game.Instance.Player.IsInCombat;
		if (ItIsLateToSwitchDlcOn = BlueprintDlc.CheckIsLateToSwitch())
		{
			ToLateReason = BlueprintDlc.ToLateReason;
		}
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
				h.HandleShowSettingsDescription(null, Title ?? "", BlueprintDlc.GetDescription() ?? "");
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

	public bool GetActualDlcState()
	{
		return BlueprintDlc?.GetDlcSwitchOnOffState() ?? false;
	}

	public void SetActualDlcState()
	{
	}

	public void ResetTempDlcState()
	{
		SetTempDlcState(GetActualDlcState());
	}

	public void ChangeValue()
	{
		if (IsSaveAllowed && !GetActualDlcState() && !ItIsLateToSwitchDlcOn)
		{
			SetTempDlcState(!DlcSwitchState.Value);
			return;
		}
		string message = ((ItIsLateToSwitchDlcOn && !GetActualDlcState()) ? ToLateReason : ((string)((!IsSaveAllowed && !GetActualDlcState()) ? UIStrings.Instance.DlcManager.CannotChangeDlcSwitchStateRightNowBecauseSaveNotAllowed : UIStrings.Instance.DlcManager.CannotChangeDlcSwitchState)));
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(message, addToLog: true, WarningNotificationFormat.Attention);
		});
	}

	private void SetTempDlcState(bool state)
	{
		DlcSwitchState.Value = state;
		bool flag = state != GetActualDlcState();
		WarningResaveGame.Value = flag;
		m_CheckModNeedToResaveCommand.Execute(flag);
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
	}
}
