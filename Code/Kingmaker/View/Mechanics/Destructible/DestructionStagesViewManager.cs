using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

public class DestructionStagesViewManager : MonoBehaviour, IDestructionStagesManager
{
	[Serializable]
	private class StageSettings
	{
		public DestructionStage Type;

		public GameObject StaticPrefab;

		public GameObject FXOnEnter;

		[AkEventReference]
		public string SFXOnEnter;
	}

	private StageSettings m_CurrentStage;

	public float SwitchPrefabsDelaySeconds;

	[SerializeField]
	private StageSettings[] m_Stages = new StageSettings[0];

	[CanBeNull]
	private MapObjectView m_MapObject;

	[CanBeNull]
	private CancellationTokenSource m_SwitchStagesCancellation;

	public IEnumerable<DestructionStage> Stages => m_Stages.Select((StageSettings i) => i.Type);

	private void Awake()
	{
		if (Application.isPlaying)
		{
			m_MapObject = this.GetComponentNonAlloc<MapObjectView>() ?? base.transform.parent.GetComponentNonAlloc<MapObjectView>();
		}
	}

	private void Start()
	{
		StageSettings[] stages = m_Stages;
		foreach (StageSettings stageSettings in stages)
		{
			if (!(stageSettings.StaticPrefab == null))
			{
				stageSettings.StaticPrefab.SetActive(m_CurrentStage == stageSettings);
			}
		}
	}

	private void OnDisable()
	{
		m_SwitchStagesCancellation?.Cancel();
		m_SwitchStagesCancellation = null;
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		StageSettings currentStage = m_CurrentStage;
		if (currentStage == null || currentStage.Type != stage)
		{
			StageSettings stageSettings = ChooseStageSettings(stage);
			if (stageSettings != null)
			{
				m_SwitchStagesCancellation?.Cancel();
				m_SwitchStagesCancellation = new CancellationTokenSource();
				SwitchStages(m_MapObject, m_CurrentStage, stageSettings, onLoad, SwitchPrefabsDelaySeconds.Seconds(), m_SwitchStagesCancellation.Token);
				m_CurrentStage = stageSettings;
			}
		}
	}

	private static async void SwitchStages(MapObjectView mapObject, [CanBeNull] StageSettings prevStage, StageSettings newStage, bool onLoad, TimeSpan switchPrefabDelay, CancellationToken ct)
	{
		if (!onLoad)
		{
			if (newStage.SFXOnEnter != null && mapObject != null && mapObject.gameObject != null)
			{
				SoundEventsManager.PostEvent(newStage.SFXOnEnter, mapObject.gameObject);
			}
			if (newStage.FXOnEnter != null)
			{
				bool flag = newStage.FXOnEnter.GetComponentsInChildren<SoundFx>().Length != 0;
				if (mapObject != null && mapObject.gameObject != null)
				{
					GameObject gameObject = FxHelper.SpawnFxOnGameObject(newStage.FXOnEnter, mapObject.gameObject);
					if (flag && gameObject != null)
					{
						SoundFx[] componentsInChildren = gameObject.GetComponentsInChildren<SoundFx>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].BlockSoundFXPlaying = true;
						}
					}
				}
			}
			await Task.Delay((int)switchPrefabDelay.TotalMilliseconds, ct);
		}
		if (prevStage != null && prevStage.StaticPrefab != null)
		{
			prevStage.StaticPrefab.SetActive(value: false);
		}
		if (newStage == null || newStage.StaticPrefab == null)
		{
			UberDebug.LogError($"Destructible view error! NewStage = {newStage}, static = {newStage?.StaticPrefab}");
		}
		if (newStage != null && newStage.StaticPrefab != null)
		{
			newStage.StaticPrefab.SetActive(value: true);
		}
		mapObject.Or(null)?.ReinitHighlighterMaterials();
	}

	private StageSettings ChooseStageSettings(DestructionStage desiredStage)
	{
		if (m_Stages == null)
		{
			return null;
		}
		if (m_Stages.Length == 0)
		{
			return null;
		}
		StageSettings stageSettings = null;
		StageSettings[] stages = m_Stages;
		foreach (StageSettings stageSettings2 in stages)
		{
			if (stageSettings2.Type == desiredStage)
			{
				return stageSettings2;
			}
			if (stageSettings == null || ((stageSettings2.Type < desiredStage) ? (stageSettings2.Type > stageSettings.Type) : (stageSettings2.Type < stageSettings.Type)))
			{
				stageSettings = stageSettings2;
			}
		}
		return stageSettings;
	}

	string IDestructionStagesManager.get_name()
	{
		return base.name;
	}
}
