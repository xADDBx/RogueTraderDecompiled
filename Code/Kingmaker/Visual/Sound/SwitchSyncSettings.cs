using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[TypeId("b5a26c15f01f5ac4d99cc613ea547f44")]
public class SwitchSyncSettings : GameSyncSettings
{
	[SerializeField]
	[AkSwitchGroupReference]
	private string switchGroup;

	[SerializeField]
	private PropertyCalculator bindingProperty;

	public override string GetCaption()
	{
		return $"[SwitchGroup] {switchGroup} set to {bindingProperty}";
	}

	public override void Sync(PropertyContext context, uint playingId)
	{
		uint value = (uint)bindingProperty.GetValue(context);
		AkSoundEngine.SetSwitch(AkUtilities.ShortIDGenerator.Compute(switchGroup), value, AkSoundEngine.GetGameObjectFromPlayingID(playingId));
	}
}
