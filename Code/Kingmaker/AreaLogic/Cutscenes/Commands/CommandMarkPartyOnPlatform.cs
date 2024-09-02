using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("3778265854fc4a5daecd35bd6688c2e8")]
public class CommandMarkPartyOnPlatform : CommandBase
{
	private class Data
	{
		public PlatformObjectEntity Platform;
	}

	[AllowedEntityType(typeof(PlatformObjectView))]
	[ValidateNotEmpty]
	public EntityReference PlatformReference;

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

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
		return "Mark party <b>on platform</b>";
	}
}
