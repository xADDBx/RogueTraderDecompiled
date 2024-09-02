namespace Kingmaker.Controllers.FX;

public abstract class SFXWrapper
{
	public virtual SoundPriority Priority { get; }

	public abstract void StartEvent();

	public abstract void StopEvent();
}
