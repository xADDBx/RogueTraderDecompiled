using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Visual.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.Blueprints;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

public class ProjectileController : IControllerTick, IController, IControllerStop
{
	private class TickProjectilesNow : ContextFlag<TickProjectilesNow>
	{
	}

	private readonly HashSet<Projectile> m_Projectiles = new HashSet<Projectile>();

	private readonly List<Projectile> m_NewProjectiles = new List<Projectile>();

	private bool m_NeedCleanup;

	private Transform m_ViewParent;

	private Vector3? m_PrevCastFxPosition;

	public IEnumerable<Projectile> Projectiles
	{
		get
		{
			using (ContextData<TickProjectilesNow>.Request())
			{
				foreach (Projectile projectile in m_Projectiles)
				{
					yield return projectile;
				}
			}
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (m_NeedCleanup)
		{
			m_Projectiles.RemoveWhere(delegate(Projectile projectile)
			{
				if (projectile.IsAlive)
				{
					return false;
				}
				GameObjectsPool.Release(projectile.View);
				projectile.OnRelease();
				return true;
			});
			m_NeedCleanup = false;
		}
		foreach (Projectile newProjectile in m_NewProjectiles)
		{
			m_Projectiles.Add(newProjectile);
		}
		m_NewProjectiles.Clear();
		using (ContextData<TickProjectilesNow>.Request())
		{
			foreach (Projectile projectile in m_Projectiles)
			{
				try
				{
					projectile.Tick();
				}
				catch (Exception ex)
				{
					projectile.Cleared = true;
					PFLog.Default.Error($"Projectile throws exception and will be removed (launcher: {projectile.Launcher}, {projectile.Blueprint})");
					PFLog.Default.Exception(ex);
				}
				m_NeedCleanup |= projectile.Destroyed || projectile.Cleared;
			}
		}
		m_PrevCastFxPosition = null;
	}

	public void Launch(Projectile projectile, Vector3? launchPosition = null)
	{
		CreateView(projectile, launchPosition);
		AddProjectile(projectile);
		projectile.OnLaunch();
	}

	private void AddProjectile(Projectile p)
	{
		if (!p.View)
		{
			PFLog.Default.Error($"Projectile's view is missing, skip ({p.Blueprint.name}, launcher: {p.Launcher})");
			return;
		}
		if ((bool)ContextData<TickProjectilesNow>.Current)
		{
			m_NewProjectiles.Add(p);
		}
		else
		{
			m_Projectiles.Add(p);
		}
		EventBus.RaiseEvent(delegate(IProjectileLaunchedHandler h)
		{
			h.HandleProjectileLaunched(p);
		});
	}

	private void CreateView(Projectile projectile, Vector3? customLaunchPosition = null)
	{
		using (ProfileScope.New("Create Projectile View"))
		{
			ProjectileView projectileView;
			using (ProfileScope.New("Load View Prefab"))
			{
				projectileView = projectile.Blueprint.View?.Load();
			}
			if (projectileView == null)
			{
				PFLog.Default.Error("Projectile's view prefab is missing ({0}, {1})", projectile.Blueprint.name, projectile.Blueprint.View.AssetId);
				return;
			}
			if (!m_ViewParent)
			{
				m_ViewParent = new GameObject("__Projectiles__").transform;
			}
			MechanicEntityView launcherView = projectile.Launcher.Entity?.View;
			Transform transform;
			Vector3 vector;
			if (!customLaunchPosition.HasValue)
			{
				transform = FindSourceTransform(projectile, launcherView);
				vector = ((transform != null) ? transform.position : ((projectile.Blueprint.IgnoreGrid && projectile.Launcher.Entity != null) ? projectile.Launcher.Entity.Position : projectile.Launcher.Point));
			}
			else
			{
				transform = null;
				vector = customLaunchPosition.Value;
			}
			using (ProfileScope.New("BeforeLaunch"))
			{
				projectile.LaunchPosition = vector;
				projectile.BeforeLaunch();
			}
			Vector3 vector2 = projectile.Launcher.Point + (projectile.Blueprint.IgnoreGrid ? Vector3.zero : new Vector3(0f, vector.y, 0f));
			bool num = projectile.Ability?.FXSettings?.SoundFXSettings?.DopplerStart != null;
			GameObject gameObject = GameObjectsPool.Claim(projectileView.gameObject, vector, Quaternion.LookRotation(projectile.GetTargetPoint() - vector2));
			if (num && gameObject != null && gameObject.TryGetComponent<SoundFx>(out var component))
			{
				component.BlockSoundFXPlaying = true;
			}
			projectile.AttachView(gameObject);
			using (ProfileScope.New("ApplyLightProbeAnchor"))
			{
				ApplyLightProbeAnchor(projectile.View);
			}
			using (ProfileScope.New("SpawnRadialBlur"))
			{
				projectile.View.GetComponentInChildren<RadialBlurSetup>().Or(null)?.SpawnFxOnGameObject(projectile.View);
			}
			if (projectile.Blueprint.UseSourceBoneScale && transform != null)
			{
				projectile.View.transform.localScale = transform.lossyScale;
			}
			projectile.View.transform.SetParent(m_ViewParent, worldPositionStays: true);
			using (ProfileScope.New("SpawnRays"))
			{
				RayView[] componentsInChildren = projectile.View.GetComponentsInChildren<RayView>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Initialize(projectile);
				}
			}
			using (ProfileScope.New("SpawnCastFx"))
			{
				GameObject gameObject2 = projectile.Blueprint.CastFx?.Load();
				if (gameObject2 != null)
				{
					Vector3 value = vector;
					Vector3? prevCastFxPosition = m_PrevCastFxPosition;
					if (value != prevCastFxPosition)
					{
						GameObject obj = ((transform != null) ? FxHelper.SpawnFxOnGameObject(gameObject2, transform.gameObject) : FxHelper.SpawnFxOnPoint(gameObject2, vector));
						obj.transform.rotation = projectile.View.transform.rotation;
						obj.EnsureComponent<AutoDestroy>().Lifetime = projectile.Blueprint.CastFxLifetime;
						m_PrevCastFxPosition = vector;
					}
				}
			}
		}
	}

	[CanBeNull]
	private static Transform FindSourceTransform(Projectile projectile, [CanBeNull] MechanicEntityView launcherView)
	{
		if (launcherView == null)
		{
			return null;
		}
		MechanicEntity entityData = launcherView.EntityData;
		if (entityData?.GetOptional<UnitPartTrapActor>() != null)
		{
			return launcherView.ViewTransform;
		}
		StarshipView starshipView = launcherView.Or(null)?.GetComponentInChildren<StarshipView>();
		BlueprintAbilityFXSettings blueprintAbilityFXSettings = projectile.Ability?.FXSettings;
		SnapMapBase snapMapBase = ((starshipView != null) ? starshipView.particleSnapMap : ((!(blueprintAbilityFXSettings?.VisualFXSettings?.ProjectileOriginIsWeapon).GetValueOrDefault()) ? ((SnapMapBase)(launcherView.Or(null)?.ParticlesSnapMap)) : ((SnapMapBase)(((projectile.Ability?.Weapon?.HoldingSlot ?? entityData?.GetFirstWeapon()?.HoldingSlot) as WeaponSlot)?.FxSnapMap))));
		if (snapMapBase != null)
		{
			BlueprintFxLocatorGroup group = blueprintAbilityFXSettings?.VisualFXSettings?.ProjectileOrigin;
			IReadOnlyList<FxBone> locators = snapMapBase.GetLocators(group);
			if (locators != null && locators.Count > 1)
			{
				if (blueprintAbilityFXSettings?.VisualFXSettings?.UseRandomLocatorInGroup ?? true)
				{
					int index = ((projectile.Ability?.Weapon == null) ? PFStatefulRandom.Visuals.Fx.Range(0, locators.Count) : projectile.Ability.Weapon.ProjectileLocatorIndexSequence.Next(locators.Count, PFStatefulRandom.Visuals.Fx));
					return locators[index].Transform;
				}
				if (projectile.Ability?.Weapon != null)
				{
					projectile.Ability.Weapon.CurrentUsedBarrel++;
					return locators[(projectile.Ability?.Weapon?.CurrentUsedBarrel % locators.Count).GetValueOrDefault()].Transform;
				}
			}
			return snapMapBase.GetLocatorFirst(group)?.Transform;
		}
		if (!launcherView.IsVisible && launcherView is UnitEntityView unitEntityView && unitEntityView.Data.CutsceneControlledUnit != null)
		{
			return launcherView.ViewTransform;
		}
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = blueprintAbilityFXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings != null && blueprintAbilityVisualFXSettings.ProjectileOriginIsWeapon)
		{
			PFLog.Default.ErrorWithReport(launcherView, $"ProjectileController.CreateView: missing ParticleSnapMap for weapon of {projectile.Launcher.Entity}");
		}
		else
		{
			PFLog.Ability.Warning(launcherView, $"ProjectileController.CreateView: missing ParticleSnapMap for {projectile.Launcher.Entity}");
		}
		return null;
	}

	private static void ApplyLightProbeAnchor(GameObject particleEffect)
	{
		particleEffect.EnsureComponent<ProbeAnchorContoller>();
	}

	public void Clear()
	{
		if ((bool)m_ViewParent)
		{
			UnityEngine.Object.Destroy(m_ViewParent.gameObject);
		}
		foreach (Projectile projectile in m_Projectiles)
		{
			projectile.Cleared = true;
		}
		m_Projectiles.Clear();
	}

	public void OnStop()
	{
		Clear();
	}
}
