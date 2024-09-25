using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.Units;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem;

[Serializable]
public class DialogSpeaker
{
	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("Blueprint")]
	private BlueprintUnitReference m_Blueprint;

	[SerializeField]
	[HideIf("m_NoSpeaker")]
	[JsonProperty("MoveCamera")]
	private bool m_MoveCamera = true;

	public bool NotRevealInFoW;

	[SerializeField]
	[JsonProperty("NoSpeaker")]
	private bool m_NoSpeaker;

	public bool DoNotReplaceSpeakerWithErrorSpeaker;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("SpeakerPortrait")]
	private BlueprintUnitReference m_SpeakerPortrait;

	public BlueprintUnit Blueprint => m_Blueprint?.Get();

	public bool MoveCamera
	{
		get
		{
			if (!m_NoSpeaker)
			{
				return m_MoveCamera;
			}
			return false;
		}
	}

	public bool NoSpeaker => m_NoSpeaker;

	public bool ReplacedSpeakerWithErrorSpeaker { get; set; }

	public BaseUnitEntity ErrorSpeaker => Game.Instance.DefaultUnit;

	public BlueprintUnit SpeakerPortrait => m_SpeakerPortrait?.Get();

	public bool NeedsEntity => Blueprint != null;

	[CanBeNull]
	public BaseUnitEntity GetSpeaker([CanBeNull] BaseUnitEntity defaultSpeaker, [CanBeNull] BlueprintCueBase cue = null)
	{
		if (NoSpeaker)
		{
			return null;
		}
		if (NeedsEntity)
		{
			return GetEntity(cue);
		}
		return defaultSpeaker;
	}

	[CanBeNull]
	public BaseUnitEntity GetEntity([CanBeNull] BlueprintCueBase cue = null)
	{
		if (Blueprint == null)
		{
			return null;
		}
		Vector3 dialogPosition = Game.Instance.DialogController.DialogPosition;
		IEnumerable<BaseUnitEntity> second = Game.Instance.EntitySpawner.CreationQueue.Select((EntitySpawnController.SpawnEntry ce) => ce.Entity).OfType<BaseUnitEntity>();
		MakeEssentialCharactersConscious();
		ReplacedSpeakerWithErrorSpeaker = false;
		BaseUnitEntity baseUnitEntity = (from u in Game.Instance.State.AllBaseUnits.Concat(Game.Instance.Player.Party)
			where u.IsInGame && !u.Suppressed
			select u).Concat(second).Select(SelectMatchingUnit).NotNull()
			.Distinct()
			.Nearest(dialogPosition);
		if (baseUnitEntity != null)
		{
			return baseUnitEntity;
		}
		string message = "speaker[" + Blueprint.name + "] doesnt exist. Skipping Cue";
		if (SpeakerPortrait != null || Blueprint.IsCompanion || DoNotReplaceSpeakerWithErrorSpeaker)
		{
			DialogDebug.Add(cue, message);
			return null;
		}
		baseUnitEntity = ErrorSpeaker;
		ReplacedSpeakerWithErrorSpeaker = true;
		message = "speaker[" + Blueprint.name + "] doesnt exist, replaced with defaultUnit";
		DialogDebug.Add(cue, message, Color.red);
		return baseUnitEntity;
	}

	[CanBeNull]
	private BaseUnitEntity SelectMatchingUnit(BaseUnitEntity unit)
	{
		BaseUnitEntity baseUnitEntity = null;
		if (unit.Blueprint == Blueprint)
		{
			baseUnitEntity = unit;
		}
		if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsConscious && !baseUnitEntity.LifeState.IsFinallyDead)
		{
			UnitReturnToConsciousController.MakeUnitConscious(baseUnitEntity);
		}
		if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsDead)
		{
			return baseUnitEntity;
		}
		return null;
	}

	private void MakeEssentialCharactersConscious()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.Blueprint == Blueprint && item.IsEssentialForGame && !item.LifeState.IsConscious)
			{
				UnitReturnToConsciousController.MakeUnitConscious(item);
			}
		}
	}
}
