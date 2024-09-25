using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class ColorRampAttribute : PropertyAttribute
{
	public ColorRampType Type { get; private set; }

	public ColorRampAttribute(ColorRampType type)
	{
		Type = type;
	}
}
