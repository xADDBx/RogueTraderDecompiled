using System;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Tutorial;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[ComponentName("Actions/Play cutscene")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("507aef8c6c6218c49aaf0987b355f400")]
public class PlayCutscene : GameAction, ICutsceneReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Cutscene")]
	private CutsceneReference m_Cutscene;

	private CutscenePlayerView m_CutscenePlayerView;

	[ShowIf("PutInQueue")]
	public bool PutInQueue;

	[ShowIf("PutInQueue")]
	public bool CheckExistence = true;

	public ParametrizedContextSetter Parameters;

	public Cutscene Cutscene
	{
		get
		{
			return m_Cutscene?.Get();
		}
		set
		{
			m_Cutscene = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<CutsceneReference>();
		}
	}

	public CutscenePlayerData CutsceneData => m_CutscenePlayerView?.PlayerData;

	public override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			throw new Exception("The cutscene can't be started from the preview unit!");
		}
		if ((bool)ContextData<TutorialIsActiveContext>.Current)
		{
			throw new Exception("The cutscene can't be started from the tutorial!");
		}
		if (PutInQueue && CheckExistence)
		{
			CutscenePlayerData cutscenePlayerData = CutscenePlayerData.Queue.FirstOrDefault((CutscenePlayerData c) => c.PlayActionId == name);
			if (cutscenePlayerData != null)
			{
				cutscenePlayerData.PreventDestruction = true;
				cutscenePlayerData.Stop();
				cutscenePlayerData.PreventDestruction = false;
			}
		}
		SceneEntitiesState state = ContextData<SpawnedUnitData>.Current?.State;
		m_CutscenePlayerView = CutscenePlayerView.Play(Cutscene, Parameters, PutInQueue, state);
		m_CutscenePlayerView.PlayerData.PlayActionId = name;
	}

	public override string GetCaption()
	{
		return string.Format("Play scene {0}", Cutscene ? Cutscene.name : "??");
	}

	public bool GetUsagesFor(Cutscene cutscene)
	{
		return cutscene == Cutscene;
	}
}
