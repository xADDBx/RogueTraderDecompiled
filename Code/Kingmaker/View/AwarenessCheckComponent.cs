using UnityEngine;

namespace Kingmaker.View;

[RequireComponent(typeof(EntityViewBase))]
[DisallowMultipleComponent]
public class AwarenessCheckComponent : MonoBehaviour
{
	public int DC;

	public float Radius;
}
