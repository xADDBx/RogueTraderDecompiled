using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("a07b97ed760cca9409b22e2e3ebc622f")]
public class CommandAction : CommandBase
{
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
			Action.Run();
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
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
