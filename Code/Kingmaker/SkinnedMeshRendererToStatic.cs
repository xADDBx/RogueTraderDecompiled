using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker;

public class SkinnedMeshRendererToStatic : MonoBehaviour
{
	private string myName = "";

	private void Start()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		SkinnedMeshRenderer component2 = GetComponent<SkinnedMeshRenderer>();
		Character component3 = base.transform.parent.parent.GetComponent<Character>();
		if ((bool)component2)
		{
			component2.enabled = false;
		}
		if ((bool)component3)
		{
			component3.enabled = false;
		}
		if ((bool)component)
		{
			component.enabled = true;
		}
	}
}
