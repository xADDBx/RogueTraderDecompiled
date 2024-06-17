using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("a2ea6d1bf82c4ae1a1b21f789d09a91a")]
public class EtudeBracketOverrideAreaMusicSetting : EtudeBracketTrigger, IHashable
{
	public AkStateReference OverrideMusicSetting;

	protected override void OnExit()
	{
		if (SoundState.Instance?.MusicStateHandler != null)
		{
			SoundState.Instance?.MusicStateHandler.DisableOverrideAreaSetting();
		}
	}

	protected override void OnEnter()
	{
		if (SoundState.Instance?.MusicStateHandler != null)
		{
			SoundState.Instance?.MusicStateHandler.OverrideAreaSetting(OverrideMusicSetting);
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
