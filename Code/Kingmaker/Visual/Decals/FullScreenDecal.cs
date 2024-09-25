using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

[ExecuteInEditMode]
public class FullScreenDecal : ScreenSpaceDecal
{
	public override bool IsFullScreen => true;

	public override DecalType Type => DecalType.Default;

	protected override void Update()
	{
		if (m_Material != null)
		{
			m_Material.SetFloat(ShaderProps._FullScreenDecal, 1f);
			m_Material.SetFloat(ShaderProps._SubstractAlphaFlag, 0f);
			m_Material.SetFloat(ShaderProps._ZTest, 0f);
			m_Material.SetFloat(ShaderProps._CullMode, 0f);
		}
	}

	protected override void OnDrawGizmosSelected()
	{
	}
}
