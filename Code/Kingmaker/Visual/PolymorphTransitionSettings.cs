using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.Visual;

[CreateAssetMenu(menuName = "Polymorph transition settings")]
public class PolymorphTransitionSettings : ScriptableObject
{
	public Polymorph.VisualTransitionSettings EnterTransition;

	public Polymorph.VisualTransitionSettings ExitTransition;
}
