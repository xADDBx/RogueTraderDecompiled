using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation;
using UnityEngine.Playables;

namespace Kingmaker.Controllers.UnityEventsReplacements;

public class AnimationManagerController : IControllerTick, IController
{
	private readonly UpdatableQueue<AnimationManager> m_AnimationManagers = new UpdatableQueue<AnimationManager>();

	public void Subscribe(AnimationManager animationManager)
	{
		m_AnimationManagers.Add(animationManager);
	}

	public void Unsubscribe(AnimationManager animationManager)
	{
		m_AnimationManagers.Remove(animationManager);
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		float unscaledDeltaTime = Game.Instance.RealTimeController.SystemDeltaTime;
		float deltaTime = Game.Instance.TimeController.GameDeltaTime;
		bool isSimulationTick = Game.Instance.TimeController.IsSimulationTick;
		m_AnimationManagers.Prepare();
		AnimationManager value;
		while (m_AnimationManagers.Next(out value))
		{
			try
			{
				TickOnAnimationManager(value);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		void TickOnAnimationManager(AnimationManager animationManager)
		{
			if (!(animationManager == null) && animationManager.PlayableGraph.IsValid())
			{
				DirectorUpdateMode timeUpdateMode = animationManager.PlayableGraph.GetTimeUpdateMode();
				if (timeUpdateMode == DirectorUpdateMode.UnscaledGameTime || isSimulationTick)
				{
					float dt = ((timeUpdateMode == DirectorUpdateMode.UnscaledGameTime) ? unscaledDeltaTime : (deltaTime * animationManager.DefaultMixerSpeed));
					animationManager.CustomUpdate(dt);
				}
			}
		}
	}
}
