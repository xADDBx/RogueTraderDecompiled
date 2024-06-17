using UnityEngine;

namespace Kingmaker.SpaceCombat.ChildForwardProceduralAnimation;

[RequireComponent(typeof(Animation))]
public class ChildForwardProceduralAnimation : MonoBehaviour
{
	public float TimeOfAppearance = 1.5f;

	public float StartOffset = 1f;

	private Animation m_Animation;

	private const float PERCENTAGE = 0.6f;

	private void Awake()
	{
		m_Animation = base.gameObject.GetComponent<Animation>();
		m_Animation.playAutomatically = false;
		AnimationClip animationClip = new AnimationClip();
		animationClip.legacy = true;
		float num = TimeOfAppearance * 0.6f / (float)base.transform.childCount;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			Vector3 localPosition = child.localPosition;
			string relativePath = child.name;
			float timeStart = num * (float)i;
			animationClip.SetCurve(curve: AnimationCurve.Linear(timeStart, localPosition.x, TimeOfAppearance, localPosition.x), relativePath: relativePath, type: typeof(Transform), propertyName: "localPosition.x");
			animationClip.SetCurve(curve: AnimationCurve.Linear(timeStart, localPosition.y, TimeOfAppearance, localPosition.y), relativePath: relativePath, type: typeof(Transform), propertyName: "localPosition.y");
			animationClip.SetCurve(curve: AnimationCurve.Linear(timeStart, localPosition.z - StartOffset, TimeOfAppearance, localPosition.z), relativePath: relativePath, type: typeof(Transform), propertyName: "localPosition.z");
			AnimationCurve curve4 = AnimationCurve.Linear(timeStart, 0f, TimeOfAppearance, 1f);
			animationClip.SetCurve(relativePath, typeof(GameObject), "m_IsActive", curve4);
		}
		m_Animation.AddClip(animationClip, "ProceduralAnimation");
	}

	private void Start()
	{
		m_Animation.Play("ProceduralAnimation");
	}
}
