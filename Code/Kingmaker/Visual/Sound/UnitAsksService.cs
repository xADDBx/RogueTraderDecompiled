using System;
using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.Visual.Sound;

public class UnitAsksService : IService, IDisposable
{
	private static ServiceProxy<UnitAsksService> s_InstanceProxy;

	private readonly List<IUnitAsksController> m_Controllers = new List<IUnitAsksController>();

	private readonly List<ITickUnitAsksController> m_TickControllers = new List<ITickUnitAsksController>();

	public static UnitAsksService Instance
	{
		get
		{
			Ensure();
			return s_InstanceProxy.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public static void Ensure()
	{
		if (s_InstanceProxy?.Instance == null)
		{
			if (Services.GetInstance<UnitAsksService>() == null)
			{
				Services.RegisterServiceInstance(new UnitAsksService());
			}
			s_InstanceProxy = Services.GetProxy<UnitAsksService>();
		}
	}

	private UnitAsksService()
	{
		EventBus.Subscribe(this);
		Register(new AggroAsksController());
		Register(new MoveAsksController());
		Register(new SkillCheckAsksController());
		Register(new PsychicPhenomenaAsksController());
		Register(new AwarenessAsksController());
		Register(new HitAsksController());
		Register(new SelectionAsksController());
		Register(new CastAsksController());
		Register(new ItemInteractionsAsksController());
		Register(new LifeStateAsksController());
		Register(new SpaceCombatAsksController());
		foreach (IUnitAsksController controller in m_Controllers)
		{
			if (controller is ITickUnitAsksController item)
			{
				m_TickControllers.Add(item);
			}
		}
	}

	private void Register(IUnitAsksController controller)
	{
		m_Controllers.Add(controller);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
		foreach (IUnitAsksController controller in m_Controllers)
		{
			controller.Dispose();
		}
	}

	public void Tick()
	{
		foreach (ITickUnitAsksController tickController in m_TickControllers)
		{
			tickController.Tick();
		}
	}
}
