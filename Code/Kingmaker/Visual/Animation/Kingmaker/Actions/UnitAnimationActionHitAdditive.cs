using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionHitAdditive", menuName = "Animation Manager/Actions/Unit Hit Additive")]
public class UnitAnimationActionHitAdditive : UnitAnimationActionHit
{
	public override bool IsAdditive => true;
}
