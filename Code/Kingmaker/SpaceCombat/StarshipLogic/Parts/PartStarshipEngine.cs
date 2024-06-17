using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarshipEngine : StarshipPart, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipEngine>, IEntityPartOwner
	{
		PartStarshipEngine Engine { get; }
	}

	private class EngineSoundHandler
	{
		private uint? engineSoundId;

		private StarshipEntity starship;

		private bool m_IsMoving;

		private bool m_IsTurnedOn;

		private float m_HealthPercent;

		private BlueprintStarshipSoundSettings settings;

		public EngineSoundHandler(StarshipEntity starship)
		{
			this.starship = starship;
			settings = ((!(Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem)) ? starship.Blueprint.SoundSettings : (starship.Blueprint.GetComponent<BlueprintStarshipStarSystemSettingsComponent>()?.SoundSettings ?? starship.Blueprint.SoundSettings));
		}

		public void Play()
		{
			if (!engineSoundId.HasValue)
			{
				engineSoundId = settings?.Play(starship);
				settings?.SetParamMovementStatus(engineSoundId.Value, m_IsMoving);
				settings?.SetParamEngineStatus(starship, m_IsTurnedOn);
				settings?.SetParamStarshipHealth(engineSoundId.Value, m_HealthPercent);
			}
		}

		public void Stop()
		{
			if (engineSoundId.HasValue)
			{
				settings?.Stop(engineSoundId.Value);
				engineSoundId = null;
			}
		}

		public void SetParamEngineStatus(bool isTurnedOn)
		{
			if (m_IsTurnedOn != isTurnedOn)
			{
				m_IsTurnedOn = isTurnedOn;
				settings?.SetParamEngineStatus(starship, isTurnedOn);
			}
		}

		public void SetParamMovementStatus(bool isMoving)
		{
			if (m_IsMoving != isMoving)
			{
				m_IsMoving = isMoving;
				if (engineSoundId.HasValue)
				{
					settings?.SetParamMovementStatus(engineSoundId.Value, isMoving);
				}
			}
		}

		public void SetParamStarshipHealth(float healthPercent)
		{
			if (!((double)Mathf.Abs(healthPercent - m_HealthPercent) < 0.01))
			{
				m_HealthPercent = healthPercent;
				settings?.SetParamStarshipHealth(engineSoundId.Value, healthPercent);
			}
		}
	}

	private EngineSoundHandler EngineSound;

	public void PlayEngineSound()
	{
		UpdateEngineSound(isMoving: false);
	}

	public void StopEngineSound()
	{
		EngineSound?.Stop();
	}

	public void UpdateEngineSound(bool isMoving)
	{
		if (EngineSound == null)
		{
			EngineSound = new EngineSoundHandler(base.Owner);
			EngineSound.Play();
		}
		EngineSound.SetParamMovementStatus(isMoving);
		EngineSound.SetParamEngineStatus(base.Owner.Navigation.SpeedMode != PartStarshipNavigation.SpeedModeType.FullStop);
		UpdateStarshipHealthSoundParam();
	}

	private void UpdateStarshipHealthSoundParam()
	{
		float num = base.Owner.Health.MaxHitPoints - base.Owner.Health.Damage;
		EngineSound?.SetParamStarshipHealth(num / (float)base.Owner.Health.MaxHitPoints);
	}

	protected override void OnViewWillDetach()
	{
		StopEngineSound();
		base.OnViewWillDetach();
	}

	public void HandleObjectInGameChanged()
	{
		if (base.Owner.IsInGame)
		{
			PlayEngineSound();
		}
		else
		{
			StopEngineSound();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
