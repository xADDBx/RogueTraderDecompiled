using System;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public readonly struct MaterialData : IEquatable<MaterialData>
{
	public readonly Material material;

	public readonly MaterialOverrides overrides;

	public MaterialData(Material material, MaterialOverrides overrides)
	{
		this.material = material;
		this.overrides = overrides;
	}

	public bool Equals(MaterialData other)
	{
		if (object.Equals(material, other.material))
		{
			return overrides.Equals(other.overrides);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is MaterialData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((material != null) ? material.GetHashCode() : 0) * 397) ^ overrides.GetHashCode();
	}
}
