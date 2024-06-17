using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

internal static class ProjectileHelper
{
	public static bool IsEnoughTimePassedToTraverseDistance(this Projectile projectile)
	{
		return projectile.IsEnoughTimePassedToTraverseDistance(projectile.DeterministicDistance);
	}

	public static bool IsEnoughTimePassedToTraverseDistance(this Projectile projectile, float distance)
	{
		float num = ((projectile.Speed > 0f) ? (distance / projectile.Speed) : projectile.Blueprint.MinTime);
		TimeSpan timeSpan = projectile.LaunchTime + TimeSpan.FromSeconds(num);
		return Game.Instance.TimeController.GameTime >= timeSpan;
	}

	public static float Distance(this Projectile projectile, Vector3 a, Vector3 b)
	{
		return projectile.Blueprint.Distance(a, b);
	}

	private static float Distance(this BlueprintProjectile blueprintProjectile, Vector3 a, Vector3 b)
	{
		if (blueprintProjectile.FollowTerrain)
		{
			return GeometryUtils.Distance2D(a, b);
		}
		return Vector3.Distance(a, b);
	}
}
