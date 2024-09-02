using Kingmaker.Visual.Animation.Actions;

namespace Kingmaker.Visual.Animation.Kingmaker;

public abstract class UnitAnimationAction : AnimationActionBase
{
	public abstract UnitAnimationType Type { get; }

	public virtual bool BlocksCover => false;

	public sealed override void OnStart(AnimationActionHandle handle)
	{
		OnStart((UnitAnimationActionHandle)handle);
	}

	public sealed override void OnFinish(AnimationActionHandle handle)
	{
		OnFinish((UnitAnimationActionHandle)handle);
	}

	public sealed override void OnUpdate(AnimationActionHandle handle, float deltaTime)
	{
		OnUpdate((UnitAnimationActionHandle)handle, deltaTime);
	}

	public sealed override void OnTransitionOutStarted(AnimationActionHandle handle)
	{
		OnTransitionOutStarted((UnitAnimationActionHandle)handle);
	}

	public abstract void OnStart(UnitAnimationActionHandle handle);

	public virtual void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
	}

	public virtual void OnFinish(UnitAnimationActionHandle handle)
	{
	}

	public virtual void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		base.OnTransitionOutStarted(handle);
	}

	private void OnValidate()
	{
	}
}
