using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal readonly struct PropertyIdentifier
{
	public readonly int id;

	public readonly string name;

	public PropertyIdentifier(string name)
	{
		this = default(PropertyIdentifier);
		this.name = name;
		id = Shader.PropertyToID(name);
	}
}
