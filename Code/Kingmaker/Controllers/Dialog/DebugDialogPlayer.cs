using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Random;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Controllers.Dialog;

public class DebugDialogPlayer : MonoBehaviour, IAreaActivationHandler, ISubscriber
{
	private class PositionsSelector
	{
		private readonly GameObject m_Players;

		private readonly GameObject m_Speakers;

		[CanBeNull]
		internal BaseUnitEntity FirstSpeaker;

		private int m_NextPlayerIndex = -1;

		private int m_NextSpeakerIndex = -1;

		public PositionsSelector(GameObject players, GameObject speakers)
		{
			m_Players = players;
			m_Speakers = speakers;
		}

		public void Reset()
		{
			m_NextPlayerIndex = -1;
			m_NextSpeakerIndex = -1;
			FirstSpeaker = null;
		}

		public void PlaceUnit(BaseUnitEntity unit)
		{
			GameObject gameObject;
			int num;
			if (unit.Faction.IsPlayer)
			{
				gameObject = m_Players;
				num = m_NextPlayerIndex++;
				if (!Game.Instance.Player.Party.Contains(unit))
				{
					GameHelper.AddCompanionToParty(unit);
				}
			}
			else
			{
				if (FirstSpeaker == null)
				{
					FirstSpeaker = unit;
				}
				gameObject = m_Speakers;
				num = m_NextSpeakerIndex++;
			}
			if (num < 0)
			{
				unit.Position = gameObject.transform.position;
				return;
			}
			if (num < gameObject.transform.childCount)
			{
				unit.Position = gameObject.transform.GetChild(num).position;
				return;
			}
			Vector3 vector = PFStatefulRandom.DebugDialog.insideUnitCircle.To3D() * 4f;
			unit.Position = gameObject.transform.position + vector;
		}
	}

	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("DefaultSpeaker")]
	private BlueprintUnitReference m_DefaultSpeaker;

	[ValidateNotNull]
	public GameObject Players;

	[ValidateNotNull]
	public GameObject Speakers;

	[ValidateNotNull]
	public GameObject Garage;

	[NotNull]
	private readonly Dictionary<BlueprintUnit, BaseUnitEntity> m_Units = new Dictionary<BlueprintUnit, BaseUnitEntity>();

	private PositionsSelector m_Selector;

	public BlueprintDialog Dialog
	{
		get
		{
			return m_Dialog?.Get();
		}
		set
		{
			m_Dialog = value.ToReference<BlueprintDialogReference>();
		}
	}

	public BlueprintUnit DefaultSpeaker
	{
		get
		{
			return m_DefaultSpeaker?.Get();
		}
		set
		{
			m_DefaultSpeaker = value.ToReference<BlueprintUnitReference>();
		}
	}

	private void Start()
	{
		EventBus.Subscribe(this);
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAreaActivated()
	{
		StartDialog();
	}

	public void StartDialog()
	{
		MoveUnitsToGarage();
		m_Units.Clear();
		m_Selector = new PositionsSelector(Players, Speakers);
		if (Application.isPlaying && Dialog != null)
		{
			BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
			PlaceUnit(mainCharacterEntity.Blueprint);
			PlaceAllUnits();
			if (m_Selector.FirstSpeaker != null)
			{
				DialogData data = DialogController.SetupDialogWithUnit(Dialog, m_Selector.FirstSpeaker, mainCharacterEntity);
				Game.Instance.DialogController.StartDialog(data);
			}
			else
			{
				DialogData data2 = DialogController.SetupDialogWithoutTarget(Dialog, null, mainCharacterEntity);
				Game.Instance.DialogController.StartDialog(data2);
			}
		}
	}

	public void StopDialog()
	{
		if (Application.isPlaying)
		{
			Game.Instance.DialogController.StopDialog();
		}
	}

	private void MoveUnitsToGarage()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party.ToList())
		{
			if (item != Game.Instance.Player.MainCharacterEntity)
			{
				GameHelper.RemoveCompanionFromParty(item);
			}
		}
		if (Garage == null)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit != Game.Instance.Player.MainCharacter.Entity)
			{
				allUnit.Position = Garage.transform.position;
			}
		}
	}

	private void PlaceUnit(BlueprintUnit blueprint)
	{
		if (blueprint != null && !m_Units.ContainsKey(blueprint))
		{
			BaseUnitEntity baseUnitEntity = Game.Instance.State.AllBaseUnits.FirstOrDefault((BaseUnitEntity u) => u.Blueprint == blueprint);
			if (baseUnitEntity == null)
			{
				SceneEntitiesState mainState = Game.Instance.LoadedAreaState.MainState;
				baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(blueprint, Vector3.zero, Quaternion.identity, mainState);
			}
			m_Units[blueprint] = baseUnitEntity;
			m_Selector.PlaceUnit(baseUnitEntity);
		}
	}

	private void PlaceAllUnits()
	{
		Queue<BlueprintScriptableObject> queue = new Queue<BlueprintScriptableObject>();
		HashSet<BlueprintScriptableObject> hashSet = new HashSet<BlueprintScriptableObject>();
		queue.Enqueue(Dialog);
		while (queue.Count > 0)
		{
			BlueprintScriptableObject blueprintScriptableObject = queue.Dequeue();
			hashSet.Add(blueprintScriptableObject);
			foreach (BlueprintScriptableObject nextObject in GetNextObjects(blueprintScriptableObject))
			{
				if (!hashSet.Contains(nextObject))
				{
					hashSet.Add(nextObject);
					queue.Enqueue(nextObject);
					PlaceUnits(nextObject);
				}
			}
		}
	}

	private void PlaceUnits(BlueprintScriptableObject so)
	{
		BlueprintCue blueprintCue = so as BlueprintCue;
		if ((bool)blueprintCue && !blueprintCue.Speaker.NoSpeaker)
		{
			if (blueprintCue.Speaker.Blueprint != null)
			{
				PlaceUnit(blueprintCue.Speaker.Blueprint);
			}
			else
			{
				PlaceUnit(DefaultSpeaker);
			}
		}
	}

	private static List<BlueprintScriptableObject> GetNextObjects(BlueprintScriptableObject so)
	{
		List<BlueprintScriptableObject> list = new List<BlueprintScriptableObject>();
		BlueprintDialog blueprintDialog = so as BlueprintDialog;
		if ((bool)blueprintDialog)
		{
			list.AddRange(blueprintDialog.FirstCue.Cues.Dereference());
		}
		BlueprintCue blueprintCue = so as BlueprintCue;
		if ((bool)blueprintCue)
		{
			list.AddRange(blueprintCue.Continue.Cues.Dereference());
			list.AddRange(blueprintCue.Answers.Dereference());
		}
		if (so is BlueprintBookPage blueprintBookPage)
		{
			list.AddRange(blueprintBookPage.Cues.Dereference());
			list.AddRange(blueprintBookPage.Answers.Dereference());
		}
		BlueprintCheck blueprintCheck = so as BlueprintCheck;
		if ((bool)blueprintCheck)
		{
			list.Add(blueprintCheck.Success);
			list.Add(blueprintCheck.Fail);
		}
		BlueprintCueSequence blueprintCueSequence = so as BlueprintCueSequence;
		if ((bool)blueprintCueSequence)
		{
			list.AddRange(blueprintCueSequence.Cues.Dereference());
			if ((bool)blueprintCueSequence.Exit)
			{
				list.AddRange(blueprintCueSequence.Exit.Continue.Cues.Dereference());
				list.AddRange(blueprintCueSequence.Exit.Answers.Dereference());
			}
		}
		BlueprintAnswer blueprintAnswer = so as BlueprintAnswer;
		if ((bool)blueprintAnswer)
		{
			list.AddRange(blueprintAnswer.NextCue.Cues.Dereference());
		}
		BlueprintAnswersList blueprintAnswersList = so as BlueprintAnswersList;
		if ((bool)blueprintAnswersList)
		{
			list.AddRange(blueprintAnswersList.Answers.Dereference());
		}
		return list;
	}
}
