using System;

namespace Kingmaker.Visual.Sound;

public interface ITickUnitAsksController : IUnitAsksController, IDisposable
{
	void Tick();
}
