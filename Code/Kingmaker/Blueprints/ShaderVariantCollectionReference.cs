using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints;

public class ShaderVariantCollectionReference : MonoBehaviour
{
	[SerializeField]
	[InspectorReadOnly]
	private ShaderVariantCollection m_ShaderVariantCollection;

	public void SetShaderVariantCollection(ShaderVariantCollection shaderVariantCollection)
	{
		m_ShaderVariantCollection = shaderVariantCollection;
	}
}
