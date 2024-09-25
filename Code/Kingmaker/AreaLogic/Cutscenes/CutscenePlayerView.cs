using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[KnowledgeDatabaseID("a31f060d9d48c194eafe99981b1b4c73")]
public class CutscenePlayerView : EntityViewBase, ICutscenePlayerView, IEntityViewBase
{
	public Cutscene Cutscene { get; set; }

	public CutscenePlayerData PlayerData => (CutscenePlayerData)base.Data;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutscenePlayerData(Cutscene, this));
	}

	public static CutscenePlayerView Play(Cutscene cutscene)
	{
		return Play(cutscene, null);
	}

	public static CutscenePlayerView Play(Cutscene cutscene, ParametrizedContextSetter context, bool queued = false, SceneEntitiesState state = null)
	{
		CutscenePlayerView cutscenePlayerView = new GameObject("[cutscene player " + cutscene.name + "]").AddComponent<CutscenePlayerView>();
		cutscenePlayerView.Cutscene = cutscene;
		cutscenePlayerView.UniqueId = Uuid.Instance.CreateString();
		if (state == null)
		{
			state = Game.Instance.State.LoadedAreaState.MainState;
		}
		Game.Instance.EntitySpawner.SpawnEntityWithView(cutscenePlayerView, state);
		if (context != null)
		{
			cutscenePlayerView.PlayerData.ParameterSetter = context;
			ParametrizedContextSetter.ParameterEntry[] array = context.Parameters.EmptyIfNull();
			foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
			{
				cutscenePlayerView.PlayerData.Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
			}
			foreach (KeyValuePair<string, object> additionalParam in context.AdditionalParams)
			{
				cutscenePlayerView.PlayerData.Parameters.Params[additionalParam.Key] = additionalParam.Value;
			}
		}
		cutscenePlayerView.PlayerData.Start(queued);
		return cutscenePlayerView;
	}
}
