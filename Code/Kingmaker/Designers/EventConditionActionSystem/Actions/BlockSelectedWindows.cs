using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("dea73582b0114443aa02e71d4f474cfe")]
public class BlockSelectedWindows : GameAction
{
	[Flags]
	private enum WindowsTypes
	{
		Inventory = 1,
		CharacterInfo = 2
	}

	[SerializeField]
	private bool m_Unblock;

	[SerializeField]
	[EnumFlagsAsButtons]
	private WindowsTypes m_SelectedWindows;

	public override string GetCaption()
	{
		int num = 0;
		string text = (m_Unblock ? "Unblock " : "Block ");
		if (HasFlag(WindowsTypes.Inventory))
		{
			num++;
			text += "Inventory ";
		}
		if (HasFlag(WindowsTypes.CharacterInfo))
		{
			num++;
			text += ((num > 1) ? "and CharacterInfo " : "CharacterInfo ");
		}
		return text + ((num > 1) ? "UI windows" : "UI window");
	}

	protected override void RunAction()
	{
		bool flag = false;
		if (HasFlag(WindowsTypes.Inventory))
		{
			if (m_Unblock)
			{
				Game.Instance.Player.InventoryWindowBlocked.Release();
			}
			else
			{
				Game.Instance.Player.InventoryWindowBlocked.Retain();
			}
			flag = true;
		}
		if (HasFlag(WindowsTypes.CharacterInfo))
		{
			if (m_Unblock)
			{
				Game.Instance.Player.CharacterInfoWindowBlocked.Release();
			}
			else
			{
				Game.Instance.Player.CharacterInfoWindowBlocked.Retain();
			}
			flag = true;
		}
		if (flag)
		{
			EventBus.RaiseEvent(delegate(ICanAccessSelectedWindowsHandler h)
			{
				h.HandleSelectedWindowsBlocked();
			});
		}
	}

	private bool HasFlag(WindowsTypes windowType)
	{
		return (m_SelectedWindows & windowType) != 0;
	}
}
