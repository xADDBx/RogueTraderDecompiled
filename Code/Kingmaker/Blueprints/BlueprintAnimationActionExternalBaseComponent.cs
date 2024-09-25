using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Events;

namespace Kingmaker.Blueprints;

[TypeId("e93a6fcf66bc4a1b956c60cc8fad094b")]
public abstract class BlueprintAnimationActionExternalBaseComponent : BlueprintComponent
{
	public abstract void Handle(AnimationManager animationManager, ClipEventType сlipEventType, int id);
}
