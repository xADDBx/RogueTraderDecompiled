using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Events;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[TypeId("53f69fb3d7bb4d418893e46d1bc21a1b")]
public class BlueprintAnimationActionExternalSimpleComponent : BlueprintAnimationActionExternalBaseComponent
{
	public override void Handle(AnimationManager animationManager, ClipEventType сlipEventType, int id)
	{
		ObjectExtensions.Or(animationManager.GetComponentInParent<ClipEventExternalReceiver>() ?? animationManager.GetComponent<ClipEventExternalReceiver>(), null)?.StartEvent(сlipEventType, animationManager, id);
	}
}
