using System;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[Serializable]
public class FxLocator : MonoBehaviour
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintFxLocatorGroup.Reference m_Group;

	[SerializeField]
	public SnapMapBase particleMap;

	[SerializeField]
	public FxBone Data;

	public CharacterFxBonesMap bonesMap;

	public BlueprintFxLocatorGroup Group => m_Group;

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "locator2");
	}
}
