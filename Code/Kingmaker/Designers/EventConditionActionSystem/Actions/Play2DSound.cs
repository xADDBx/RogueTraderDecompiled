using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Play2DSound")]
[AllowMultipleComponents]
[TypeId("12f52f92a7d3efe47b69ba8b41d4e47d")]
public class Play2DSound : GameAction
{
	[Tooltip("Ak event name from Wwise library")]
	public string SoundName;

	[Tooltip("Sets Ak switch on player's Sex")]
	public bool SetSex;

	[Tooltip("Sets Ak switch on player's Race")]
	public bool SetRace;

	public override void RunAction()
	{
		if (SoundName == "")
		{
			PFLog.Default.Error("Sound name is Empty. Can't play sound.", this);
			return;
		}
		GameObject gameObject = SoundState.Get2DSoundObject();
		if (SetSex)
		{
			SoundUtility.SetGenderFlags(gameObject);
		}
		if (SetRace)
		{
			SoundUtility.SetRaceFlags(gameObject);
		}
		SoundEventsManager.PostEvent(SoundName, gameObject, canBeStopped: true);
	}

	public override string GetCaption()
	{
		return $"Sound 2D ({SoundName})";
	}
}
