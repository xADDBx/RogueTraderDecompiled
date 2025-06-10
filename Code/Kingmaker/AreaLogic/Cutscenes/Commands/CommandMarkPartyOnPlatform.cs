using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("3778265854fc4a5daecd35bd6688c2e8")]
public class CommandMarkPartyOnPlatform : CommandBase
{
	private class Data
	{
		public PlatformObjectEntity Platform;

		public Dictionary<BaseUnitEntity, IKState> SavedIKStates = new Dictionary<BaseUnitEntity, IKState>();
	}

	private class IKState
	{
		public bool IKControllerEnabled;

		public bool IKControllerEnableIK;

		public bool GrounderIKEnabled;

		public bool BipedIkEnabled;

		public bool GrounderFBBIKEnabled;
	}

	[AllowedEntityType(typeof(PlatformObjectView))]
	[ValidateNotEmpty]
	public EntityReference PlatformReference;

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

	[SerializeField]
	public bool DisablePartyIK = true;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Platform = PlatformReference.FindData() as PlatformObjectEntity;
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(ExceptThese))
		{
			if (!(item2.GetValue() is BaseUnitEntity item))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {item2} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (!list.Contains(characters))
			{
				characters.GetOrCreate<EntityPartStayOnPlatform>().SetOnPlatform(commandData.Platform);
			}
		}
		if (DisablePartyIK)
		{
			DisablePartyIKComponents(commandData, list);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		List<BaseUnitEntity> value;
		using (CollectionPool<List<BaseUnitEntity>, BaseUnitEntity>.Get(out value))
		{
			foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(ExceptThese))
			{
				if (!(item2.GetValue() is BaseUnitEntity item))
				{
					string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {item2} is not BaseUnitEntity";
					if (!QAModeExceptionReporter.MaybeShowError(message))
					{
						UberDebug.LogError(message);
					}
				}
				else
				{
					value.Add(item);
				}
			}
			foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
			{
				if (!value.Contains(characters))
				{
					characters?.GetOrCreate<EntityPartStayOnPlatform>().ReleaseFromPlatform(commandData.Platform);
				}
			}
			if (DisablePartyIK)
			{
				RestorePartyIKComponents(commandData);
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		string text = (DisablePartyIK ? " + disable party IK" : "");
		return "Mark party <b>on platform</b>" + text;
	}

	private void DisablePartyIKComponents(Data data, List<BaseUnitEntity> exceptedUnits)
	{
		data.SavedIKStates.Clear();
		foreach (BaseUnitEntity characters in Game.Instance.Player.GetCharactersList(m_UnitsList))
		{
			if (exceptedUnits.Contains(characters) || characters?.View == null)
			{
				continue;
			}
			IKState iKState = new IKState();
			data.SavedIKStates[characters] = iKState;
			IKController componentInChildren = characters.View.GetComponentInChildren<IKController>();
			if (componentInChildren != null)
			{
				iKState.IKControllerEnabled = componentInChildren.enabled;
				iKState.IKControllerEnableIK = componentInChildren.EnableIK;
				componentInChildren.EnableIK = false;
				componentInChildren.enabled = false;
				if (componentInChildren.GrounderIK != null)
				{
					iKState.GrounderIKEnabled = componentInChildren.GrounderIK.enabled;
					componentInChildren.GrounderIK.enabled = false;
				}
				if (componentInChildren.BipedIk != null)
				{
					iKState.BipedIkEnabled = componentInChildren.BipedIk.enabled;
					componentInChildren.BipedIk.enabled = false;
				}
				if (componentInChildren.GrounderIk != null)
				{
					iKState.GrounderFBBIKEnabled = componentInChildren.GrounderIk.enabled;
					componentInChildren.GrounderIk.enabled = false;
				}
			}
			else
			{
				GrounderBipedIK componentInChildren2 = characters.View.GetComponentInChildren<GrounderBipedIK>();
				if (componentInChildren2 != null)
				{
					iKState.GrounderIKEnabled = componentInChildren2.enabled;
					componentInChildren2.enabled = false;
				}
				FullBodyBipedIK componentInChildren3 = characters.View.GetComponentInChildren<FullBodyBipedIK>();
				if (componentInChildren3 != null)
				{
					iKState.BipedIkEnabled = componentInChildren3.enabled;
					componentInChildren3.enabled = false;
				}
				GrounderFBBIK componentInChildren4 = characters.View.GetComponentInChildren<GrounderFBBIK>();
				if (componentInChildren4 != null)
				{
					iKState.GrounderFBBIKEnabled = componentInChildren4.enabled;
					componentInChildren4.enabled = false;
				}
			}
		}
	}

	private void RestorePartyIKComponents(Data data)
	{
		foreach (KeyValuePair<BaseUnitEntity, IKState> savedIKState in data.SavedIKStates)
		{
			BaseUnitEntity key = savedIKState.Key;
			IKState value = savedIKState.Value;
			if (key?.View == null)
			{
				continue;
			}
			IKController componentInChildren = key.View.GetComponentInChildren<IKController>();
			if (componentInChildren != null)
			{
				componentInChildren.EnableIK = value.IKControllerEnableIK;
				componentInChildren.enabled = value.IKControllerEnabled;
				if (componentInChildren.GrounderIK != null)
				{
					componentInChildren.GrounderIK.enabled = value.GrounderIKEnabled;
				}
				if (componentInChildren.BipedIk != null)
				{
					componentInChildren.BipedIk.enabled = value.BipedIkEnabled;
				}
				if (componentInChildren.GrounderIk != null)
				{
					componentInChildren.GrounderIk.enabled = value.GrounderFBBIKEnabled;
				}
				continue;
			}
			GrounderBipedIK componentInChildren2 = key.View.GetComponentInChildren<GrounderBipedIK>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.enabled = value.GrounderIKEnabled;
			}
			FullBodyBipedIK componentInChildren3 = key.View.GetComponentInChildren<FullBodyBipedIK>();
			if (componentInChildren3 != null)
			{
				componentInChildren3.enabled = value.BipedIkEnabled;
			}
			GrounderFBBIK componentInChildren4 = key.View.GetComponentInChildren<GrounderFBBIK>();
			if (componentInChildren4 != null)
			{
				componentInChildren4.enabled = value.GrounderFBBIKEnabled;
			}
		}
		data.SavedIKStates.Clear();
	}
}
