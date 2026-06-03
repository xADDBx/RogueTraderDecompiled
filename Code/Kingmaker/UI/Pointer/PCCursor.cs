using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class PCCursor : BaseCursor, IRewiredCursorController
{
	public PlayerMouse RewiredMouse;

	public static PCCursor Instance { get; private set; }

	bool IRewiredCursorController.Enabled
	{
		get
		{
			return base.IsActive;
		}
		set
		{
			SetActive(value);
		}
	}

	GameObject IRewiredCursorController.Cursor => m_CursorTransform.Or(null)?.gameObject;

	protected override void OnBind()
	{
		Instance = this;
		RewiredMouse = PlayerMouse.Factory.Create();
		Rewired.Player player = ReInput.players.GetPlayer("MainPlayer");
		RewiredMouse.playerId = player.id;
		if (!ApplicationHelper.IsRunningOnSwitch2)
		{
			RewiredMouse.useHardwarePointerPosition = true;
		}
		else if ((bool)SettingsRoot.Game.Switch.SwitchJoyConAsMouse)
		{
			RewiredMouse.useHardwarePointerPosition = false;
			RewiredMouse.xAxis.actionName = "SwitchMouseX";
			RewiredMouse.yAxis.actionName = "SwitchMouseY";
			RewiredMouse.leftButton.actionName = "RightUp";
			RewiredMouse.rightButton.actionName = "RightBottom";
			RewiredMouse.wheel.xAxis.actionName = "RightStickX";
			RewiredMouse.wheel.yAxis.actionName = "RightStickY";
			UIKitRewiredCursorController.Enabled = true;
			StandaloneInputModule.ActivateModule();
			StandaloneInputModule.AddMouseInputSource(RewiredMouse);
		}
		UIKitRewiredCursorController.SetRewiredCursorController(this);
		m_Disposable.Add(MainThreadDispatcher.UpdateAsObservable().Subscribe(OnUpdate));
	}

	protected override void OnDispose()
	{
		Instance = null;
	}

	private void OnUpdate()
	{
		base.Position = RewiredMouse.screenPosition;
	}

	protected override void SetCanFlipZoneImpl()
	{
		base.SetCanFlipZoneImpl();
		if (ApplicationHelper.IsRunningOnAnySwitch)
		{
			return;
		}
		m_CanFlipZoneText.text = UIStrings.Instance.Tooltips.FlipZoneStrategist;
		m_Disposable.Add(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.FlipZoneStrategist.name, delegate
		{
			if (CanFlipZoneAbility)
			{
				EventBus.RaiseEvent(delegate(IFlipZoneAbilityHandler h)
				{
					h.HandleFlipZoneAbility();
				});
			}
		}));
	}
}
