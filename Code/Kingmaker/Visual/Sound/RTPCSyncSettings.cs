using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[TypeId("476b16f009150454eb13a4ad939f8bb7")]
public class RTPCSyncSettings : GameSyncSettings
{
	[SerializeField]
	[AkGameParameterReference]
	private string rtpc;

	[SerializeField]
	private PropertyCalculator bindingProperty;

	public override string GetCaption()
	{
		return $"[RTPC] {rtpc} set to {bindingProperty}";
	}

	public override void Sync(PropertyContext context, uint playingId)
	{
		float in_value = (float)bindingProperty.GetValue(context) / 100f;
		AkSoundEngine.SetRTPCValueByPlayingID(rtpc, in_value, playingId);
	}
}
