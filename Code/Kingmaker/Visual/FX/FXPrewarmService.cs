using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Visual.FX;

public class FXPrewarmService : IService
{
	private Queue<PrefabLink> m_Queue = new Queue<PrefabLink>();

	private HashSet<string> m_QueuedTypes = new HashSet<string>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public void Update()
	{
		if (m_Queue.Count != 0)
		{
			PrefabLink prefabLink = m_Queue.Dequeue();
			GameObject gameObject = prefabLink.Load();
			if (!(gameObject == null))
			{
				m_QueuedTypes.Remove(prefabLink.AssetId);
				GameObjectsPool.Warmup(gameObject.GetComponent<PooledGameObject>(), 3);
			}
		}
	}

	private void TryQueue(PrefabLink link)
	{
		if (!(link == null) && link.Exists() && !m_QueuedTypes.Contains(link.AssetId))
		{
			m_Queue.Enqueue(link);
			m_QueuedTypes.Add(link.AssetId);
		}
	}

	public void PrewarmWeaponAbilities(WeaponAbilityContainer weapons)
	{
		if (weapons == null)
		{
			return;
		}
		foreach (WeaponAbility weapon in weapons)
		{
			BlueprintAbilityFXSettings blueprintAbilityFXSettings = weapon.FXSettings ?? weapon.Ability.FXSettings;
			if (blueprintAbilityFXSettings == null || blueprintAbilityFXSettings.VisualFXSettings == null)
			{
				break;
			}
			foreach (BlueprintProjectile projectile in blueprintAbilityFXSettings.VisualFXSettings.Projectiles)
			{
				if (projectile == null)
				{
					PFLog.Default.Error("No projectile for " + blueprintAbilityFXSettings.name);
					continue;
				}
				TryQueue(projectile.CastFx);
				TryQueue(projectile.ProjectileHit.HitFx);
				TryQueue(projectile.ProjectileHit.HitSnapFx);
				TryQueue(projectile.ProjectileHit.MissFx);
				TryQueue(projectile.ProjectileHit.MissDecalFx);
			}
		}
	}

	public void PrewarmWeaponSet(HandsEquipmentSet set)
	{
		PrewarmWeaponAbilities(set.PrimaryHand.MaybeWeapon?.Blueprint.WeaponAbilities);
		PrewarmWeaponAbilities(set.SecondaryHand.MaybeWeapon?.Blueprint.WeaponAbilities);
	}
}
