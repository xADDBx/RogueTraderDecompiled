using Kingmaker.Controllers.Timer;

namespace Kingmaker.AreaLogic.Etudes;

public readonly struct TimerShowCounterUIStruct
{
	public readonly PlayerTimer Timer;

	public string Id => Timer.Blueprint.AssetGuid;

	public TimerShowCounterUIStruct(PlayerTimer timer)
	{
		Timer = timer;
	}
}
