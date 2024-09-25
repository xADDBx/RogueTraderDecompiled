using UnityEngine.UI;

namespace Kingmaker.UI.Workarounds;

public class ToggleWorkaround : Toggle
{
	protected override void OnEnable()
	{
		ToggleEvent toggleEvent = onValueChanged;
		onValueChanged = null;
		bool allowSwitchOff = base.group?.allowSwitchOff ?? false;
		if ((bool)base.group)
		{
			base.group.allowSwitchOff = true;
		}
		base.OnEnable();
		if ((bool)base.group)
		{
			base.group.allowSwitchOff = allowSwitchOff;
		}
		onValueChanged = toggleEvent;
	}

	protected override void OnDisable()
	{
		ToggleEvent toggleEvent = onValueChanged;
		onValueChanged = null;
		bool allowSwitchOff = base.group?.allowSwitchOff ?? false;
		if ((bool)base.group)
		{
			base.group.allowSwitchOff = true;
		}
		base.OnDisable();
		if ((bool)base.group)
		{
			base.group.allowSwitchOff = allowSwitchOff;
		}
		onValueChanged = toggleEvent;
	}
}
