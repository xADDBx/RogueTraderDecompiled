using Kingmaker.Visual.Animation.Actions;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class Transition : ScriptableObject
{
	public AnimationActionBase FromAction;

	public AnimationClip FromClip;

	public AnimationActionBase ToAction;

	public AnimationClip ToClip;

	public float Duration;
}
