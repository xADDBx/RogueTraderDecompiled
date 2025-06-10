using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("a07b97ed760cca9409b22e2e3ebc622f")]
public class CommandAction : CommandBase
{
	private class Data
	{
		public int TicksToWait;
	}

	public class PlayerData : ContextData<PlayerData>
	{
		public CutscenePlayerData Player { get; private set; }

		public PlayerData Setup(CutscenePlayerData player)
		{
			Player = player;
			return this;
		}

		protected override void Reset()
		{
			Player = null;
		}
	}

	public ActionList Action;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		using (ContextData<PlayerData>.Request().Setup(player))
		{
			player.GetCommandData<Data>(this).TicksToWait = (Action.Actions.Any((GameAction action) => action is TeleportParty) ? 2 : 0);
			Action.Run();
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).TicksToWait <= 0;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).TicksToWait--;
	}

	public override string GetCaption()
	{
		if (Action == null || Action.Actions == null)
		{
			return "<b>Action</b> none";
		}
		GameAction gameAction = Action.Actions.FirstOrDefault();
		return "<b>Action</b> " + ((gameAction == null) ? "none" : (gameAction.GetCaption() + ((Action.Actions.Length > 1) ? (" and " + (Action.Actions.Length - 1) + " more") : "")));
	}
}
