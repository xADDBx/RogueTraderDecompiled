using System;

namespace Kingmaker.Sound;

internal static class TriggerFactory
{
	public static TriggerObjectBase CreateTrigger(this TriggerType type, AkAudioTriggerable owner)
	{
		return type switch
		{
			TriggerType.AreaLoad => new TriggerAreaLoaded(owner), 
			TriggerType.AreaUnload => new TriggerAreaUnloaded(owner), 
			TriggerType.Enabled => new TriggerEnabled(owner), 
			TriggerType.Disabled => new TriggerDisabled(owner), 
			TriggerType.ZoneEntered => new TriggerZone(owner, onEnter: true), 
			TriggerType.ZoneExited => new TriggerZone(owner, onEnter: false), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
