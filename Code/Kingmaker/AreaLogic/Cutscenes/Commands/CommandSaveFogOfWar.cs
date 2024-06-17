using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Owlcat.Runtime.Visual.FogOfWar;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[UsedImplicitly]
[TypeId("d757136f3b84436da1452e6d27a4f0bb")]
public class CommandSaveFogOfWar : CommandBase
{
	public class Data
	{
		public Task Task;

		public byte[] State;

		public FogOfWarArea Area;

		public EntityVisibilityForPlayerController Controller;

		public void Reset()
		{
			State = null;
			Area = null;
			Task = null;
			Controller = null;
		}
	}

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		FogOfWarArea active = FogOfWarArea.Active;
		if (active == null)
		{
			commandData.Reset();
			return;
		}
		try
		{
			commandData.State = null;
			commandData.Area = active;
			commandData.Task = active.RequestData().ContinueWith(delegate(Task<byte[]> task, object state)
			{
				Data data = (Data)state;
				if (task.IsCompletedSuccessfully)
				{
					data.State = task.Result;
				}
				data.Task = null;
			}, commandData);
			commandData.Controller = Game.Instance?.EntityVisibilityForPlayerController;
			if (commandData.Controller != null)
			{
				commandData.Controller.DisableVisibleEntityRevealing();
			}
		}
		catch
		{
			commandData.Reset();
			throw;
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		Data commandData = player.GetCommandData<Data>(this);
		try
		{
			if (commandData.Controller != null)
			{
				commandData.Controller.EnableVisibleEntityRevealing();
			}
			if (commandData.Area != null)
			{
				if (commandData.Task != null)
				{
					commandData.Task.Wait(1000);
				}
				if (commandData.State != null)
				{
					commandData.Area.RestoreFogOfWarMask(commandData.State);
				}
			}
		}
		finally
		{
			commandData.Reset();
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
