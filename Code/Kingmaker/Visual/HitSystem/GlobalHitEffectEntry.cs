using System;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Visual.HitSystem;

[Serializable]
public class GlobalHitEffectEntry
{
	public PrefabLink HitMarkEffect;

	public SoundFXSettings HitMarkSoundSettings;
}
