using System;
using Kingmaker.Enums;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DialogCameraPositionOffsetEntry
{
	public Size Size = Size.Medium;

	public float HeightOffset;

	public string Name => Size.ToString();
}
