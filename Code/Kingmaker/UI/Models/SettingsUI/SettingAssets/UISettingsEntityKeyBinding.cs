using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Entities;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/Controls/SettingsEntityKeyBinding")]
public class UISettingsEntityKeyBinding : UISettingsEntityWithValueBase<KeyBindingPair>
{
	[Header("Special property")]
	[Tooltip("Callback on KeyDown and KeyUp")]
	[SerializeField]
	[FormerlySerializedAs("TriggerOnHold")]
	private bool SplitPressTrigger;

	[SerializeField]
	[Tooltip("Callback on GetKey")]
	private bool IsHoldTrigger;

	private SettingsEntityKeyBindingPair m_SettingKeyBindingPair;

	private string Name => base.name;

	public SettingsEntityKeyBindingPair SettingKeyBindingPair => m_SettingKeyBindingPair;

	public override SettingsListItemType? Type => SettingsListItemType.Keybind;

	public bool IsPressed
	{
		get
		{
			if (GetBinding(0).IsPressed)
			{
				return true;
			}
			if (GetBinding(1).IsPressed)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsDown
	{
		get
		{
			if (GetBinding(0).IsDown)
			{
				return true;
			}
			if (GetBinding(1).IsDown)
			{
				return true;
			}
			return false;
		}
	}

	public override void LinkSetting(SettingsEntity<KeyBindingPair> setting)
	{
		if (setting != Setting)
		{
			base.LinkSetting(setting);
			m_SettingKeyBindingPair = setting as SettingsEntityKeyBindingPair;
			setting.OnTempValueChanged += delegate
			{
				RenewRegisteredBindings();
			};
		}
	}

	public KeyBindingData GetBinding(int index)
	{
		return index switch
		{
			0 => GetTempValue().Binding1, 
			1 => GetTempValue().Binding2, 
			_ => default(KeyBindingData), 
		};
	}

	public bool TrySetBinding(KeyBindingData value, int index)
	{
		if (!Game.Instance.Keyboard.CanBeRegistered(base.name, value.Key, GetTempValue().GameModesGroup, value.IsCtrlDown, value.IsAltDown, value.IsShiftDown))
		{
			return false;
		}
		switch (index)
		{
		default:
			return false;
		case 0:
			if (GetTempValue().Binding2.IsIdenticalNotNone(value))
			{
				return false;
			}
			m_SettingKeyBindingPair.SetTempKeyBindingData(value, 0);
			break;
		case 1:
			if (GetTempValue().Binding1.IsIdenticalNotNone(value))
			{
				return false;
			}
			m_SettingKeyBindingPair.SetTempKeyBindingData(value, 1);
			break;
		}
		return true;
	}

	public void RenewRegisteredBindings()
	{
		if (Setting == null)
		{
			UberDebug.LogError("Setting " + base.name + " is not initialized");
			return;
		}
		if (!SplitPressTrigger)
		{
			Game.Instance.Keyboard.UnregisterBinding(base.name);
		}
		else
		{
			Game.Instance.Keyboard.UnregisterBinding(base.name + UIConsts.SuffixOn);
			Game.Instance.Keyboard.UnregisterBinding(base.name + UIConsts.SuffixOff);
		}
		if (GetTempValue().Binding1.Key != 0)
		{
			Game.Instance.Keyboard.RegisterBinding(Name, GetTempValue().Binding1, GetTempValue().GameModesGroup, SplitPressTrigger, IsHoldTrigger);
		}
		if (GetTempValue().Binding2.Key != 0)
		{
			Game.Instance.Keyboard.RegisterBinding(Name, GetTempValue().Binding2, GetTempValue().GameModesGroup, SplitPressTrigger, IsHoldTrigger);
		}
		EventBus.RaiseEvent(delegate(IKeybindChanged h)
		{
			h.OnKeybindChanged();
		});
	}
}
