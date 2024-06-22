using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Sound.Base;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Play3DSound")]
[AllowMultipleComponents]
[TypeId("be3026f011f344f448094a75ed64a9f5")]
public class Play3DSound : GameAction
{
	[Tooltip("Ak event name from Wwise library")]
	public string SoundName;

	public EntityReference SoundSourceObject;

	[Tooltip("Sets Ak switch on player's Sex")]
	public bool SetSex;

	[Tooltip("Sets Ak switch on player's Race")]
	public bool SetRace;

	[Tooltip("Sets SoundSourceObject as current dialog speaker")]
	public bool SetCurrentSpeaker;

	protected override void RunAction()
	{
		if (SoundName == "")
		{
			Element.LogError(this, "Sound name is Empty. Can't play sound.");
			return;
		}
		GameObject gameObject = null;
		if (SetCurrentSpeaker)
		{
			if (Game.Instance.DialogController.CurrentSpeaker != null)
			{
				gameObject = Game.Instance.DialogController.CurrentSpeaker.View.gameObject;
			}
			if (!gameObject)
			{
				Element.LogError(this, "CurrentSpeaker is NULL");
				return;
			}
		}
		else
		{
			EntityViewBase entityViewBase = (EntityViewBase)SoundSourceObject.FindView();
			if (!entityViewBase)
			{
				Element.LogError(this, "Target object for sound play is NULL");
				return;
			}
			UnitSpawnerBase unitSpawnerBase = entityViewBase as UnitSpawnerBase;
			if (unitSpawnerBase != null)
			{
				AbstractUnitEntity spawnedUnit = unitSpawnerBase.SpawnedUnit;
				if (spawnedUnit == null)
				{
					return;
				}
				gameObject = spawnedUnit.View.gameObject;
			}
			else
			{
				gameObject = entityViewBase.gameObject;
			}
		}
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
		return "Sound 3D (" + SoundName + ")";
	}
}
