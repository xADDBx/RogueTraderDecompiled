using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Interaction;

[Serializable]
public class InteractionVariantVisualSetEntry
{
	public InteractionActorType Type;

	public Sprite Normal;

	public Sprite Highlighted;

	public Sprite Pressed;

	public Sprite Disable;

	public LocalizedString InteractionName;
}
