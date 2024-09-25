using System;
using Kingmaker.Controllers.Interfaces;

namespace Code.GameCore.Mics;

public interface ITimeController : InterfaceService
{
	TimeSpan DeltaTimeSpan { get; }

	float DeltaTime { get; }

	TimeSpan GameDeltaTimeSpan { get; }

	float GameDeltaTime { get; }

	TimeSpan GameDeltaTimeInterpolation { get; }

	float PlayerTimeScale { get; set; }

	float GameTimeScale { get; set; }

	float TimeScale { get; }

	TimeSpan GameTime { get; }

	TimeSpan RealTime { get; }

	bool IsGameDeltaTimeZero { get; }

	bool CanTick(IControllerTick controller);
}
