using Core.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility.GameConst;

namespace Kingmaker.Controllers.MapObjects;

public class InteractionHighlightController : IControllerEnable, IController, IControllerDisable
{
	private bool m_IsHighlighting;

	private bool m_Inactive;

	public static InteractionHighlightController Instance { get; private set; }

	public bool DebugHighlightCovers { get; private set; }

	public bool IsHighlighting => m_IsHighlighting;

	public void OnEnable()
	{
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOn, HighlightOn);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOff, HighlightOff);
		Instance = this;
		m_Inactive = false;
	}

	[Cheat(Name = "switch_highlight_covers", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SwitchHighlightCovers()
	{
		Instance.DebugHighlightCovers = !Instance.DebugHighlightCovers;
		Instance.UpdateHighlightCovers();
	}

	private void UpdateHighlightCovers()
	{
		if (m_Inactive)
		{
			return;
		}
		foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
		{
			mapObject.View.UpdateHighlight();
		}
	}

	public void SwitchHighlight()
	{
		Highlight(!IsHighlighting);
	}

	public void Highlight(bool on)
	{
		if (on)
		{
			HighlightOn();
		}
		else
		{
			HighlightOff();
		}
	}

	private void HighlightOn()
	{
		if (m_IsHighlighting || m_Inactive)
		{
			return;
		}
		m_IsHighlighting = true;
		foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
		{
			mapObject.View.UpdateHighlight();
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			allUnit.View.UpdateHighlight(raiseEvent: false);
		}
		EventBus.RaiseEvent(delegate(IInteractionHighlightUIHandler h)
		{
			h.HandleHighlightChange(isOn: true);
		});
	}

	private void HighlightOff()
	{
		if (!m_IsHighlighting || m_Inactive)
		{
			return;
		}
		m_IsHighlighting = false;
		foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
		{
			mapObject.View.UpdateHighlight();
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			allUnit.View.UpdateHighlight(raiseEvent: false);
		}
		EventBus.RaiseEvent(delegate(IInteractionHighlightUIHandler h)
		{
			h.HandleHighlightChange(isOn: false);
		});
	}

	public void OnDisable()
	{
		Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOn, HighlightOn);
		Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOff, HighlightOff);
		HighlightOff();
		m_Inactive = true;
		Instance = null;
	}
}
