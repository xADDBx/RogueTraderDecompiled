using System;
using Kingmaker.Enums;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SoundRoot
{
	[AkEventReference]
	public string DefaultMusicTheme;

	[AkEventReference]
	public string DefaultMusicThemeStop;

	[AkEventReference]
	public string FinishingBlow;

	[AkEventReference]
	public string SpellFailed;

	[Range(0f, 1f)]
	public float TBMIdleAudioOverride;

	public float AggroBarkRadius = 10f;

	public float LowHealthBarkHPPercent = 0.2f;

	public float LowShieldBarkPercent = 0.3f;

	public int EnemyMassDeathKillsCount = 3;

	public int TilesToBarkMoveOrderSpaceCombat = 4;

	public Size[] EnemyShipSizesToBarkEnemyDeathSC = new Size[3]
	{
		Size.Frigate_1x2,
		Size.Cruiser_2x4,
		Size.GrandCruiser_3x6
	};

	public Size[] EnemyShipSizesToBarkShieldIsDownSC = new Size[3]
	{
		Size.Frigate_1x2,
		Size.Cruiser_2x4,
		Size.GrandCruiser_3x6
	};

	public float StarSystemAudioScalingFactor = 1f;
}
