using System;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionDisableTrapSettings : InteractionSettings
{
	public override bool ShouldShowUseAnimationState => true;
}
