using System;
using Kingmaker.Visual.Animation.Actions;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class AnimationRoot
{
	public AnimatorControllerAction LocoMotion;

	public AnimationActionBase AttackMainHand;

	public AnimationActionBase EquipMainHand;

	public AnimationActionBase UnequipMainHand;

	public AnimationActionBase AttackOffHand;

	public AnimationActionBase EquipOffHand;

	public AnimationActionBase UnequipOffHand;
}
