using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Modding;

public class OwlcatModificationController : IController, IControllerEnable, IControllerDisable, IControllerTick, IControllerStart, IControllerStop
{
	[CanBeNull]
	public IController DefaultController { get; internal set; }

	void IControllerEnable.OnEnable()
	{
		OnEnableBeforeDefaultController();
		(DefaultController as IControllerEnable)?.OnEnable();
		OnEnable();
	}

	void IControllerDisable.OnDisable()
	{
		OnDisableBeforeDefaultController();
		(DefaultController as IControllerDisable)?.OnDisable();
		OnDisable();
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Any;
	}

	void IControllerTick.Tick()
	{
		TickBeforeDefaultController();
		(DefaultController as IControllerTick)?.Tick();
		Tick();
	}

	void IControllerStart.OnStart()
	{
		OnStartBeforeDefaultController();
		(DefaultController as IControllerStart)?.OnStart();
		OnStart();
	}

	void IControllerStop.OnStop()
	{
		OnStopBeforeDefaultController();
		(DefaultController as IControllerStop)?.OnStop();
		OnStop();
	}

	protected virtual void OnEnableBeforeDefaultController()
	{
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisableBeforeDefaultController()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnStartBeforeDefaultController()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnStopBeforeDefaultController()
	{
	}

	protected virtual void OnStop()
	{
	}

	protected virtual void TickBeforeDefaultController()
	{
	}

	protected virtual void Tick()
	{
	}
}
