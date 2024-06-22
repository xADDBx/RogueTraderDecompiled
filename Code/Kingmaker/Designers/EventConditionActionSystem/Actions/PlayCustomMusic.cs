using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/PlayCustomMusic")]
[AllowMultipleComponents]
[TypeId("60ce9f50487c58446aad79d52f8e5e54")]
public class PlayCustomMusic : GameAction
{
	[AkEventReference]
	public string MusicEventStart;

	[AkEventReference]
	public string MusicEventStop;

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return $"Custom music ({MusicEventStart})";
	}
}
