using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("0d949fdd9cb63b94db868d35dcd1fec7")]
public class CommandAddFact : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		public EntityFact Fact;
	}

	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		if (commandData.Unit == null)
		{
			return;
		}
		using (ContextData<CommandAction.PlayerData>.Request().Setup(player))
		{
			commandData.Fact = commandData.Unit.AddFact(Fact);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Unit != null && commandData.Fact != null)
		{
			commandData.Unit.Facts.Remove(commandData.Fact);
			commandData.Unit = null;
			commandData.Fact = null;
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
