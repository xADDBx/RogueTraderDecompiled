using System;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
public class CharacterTextureDescription
{
	public Texture2D ActiveTexture;

	public Texture2D DiffuseTexture;

	public Texture2D NormalTexture;

	public Texture2D MaskTexture;

	public Texture2D RampShadowTexture;

	public Texture2D DefaultMask1;

	public Texture2D DefaultMask2;

	public bool DiffuseActive;

	public bool NormalActive;

	public bool MaskActive;

	public bool RampShadowActive;

	public bool UseRamp1Mask;

	public bool UseRamp2Mask;

	public bool UseDefaultMask1;

	public bool UseDefaultMask2;

	public bool UseShadowMask;

	public RampMaskDescription Ramps = new RampMaskDescription();

	private static Material s_RepaintMaterial;

	private static Material s_ShadowPaintMaterial;

	public CharacterTextureChannel Channel;

	[NonSerialized]
	internal bool IsEmpty;

	[NonSerialized]
	internal Material Material;

	[SerializeField]
	private Texture2D m_Texture => ActiveTexture;

	public Material GetMaterial()
	{
		return Material;
	}

	public CharacterTextureDescription()
	{
	}

	public CharacterTextureDescription(CharacterTextureChannel channel, Texture2D mainTexture)
	{
		Channel = channel;
	}

	public Texture GetSourceTexture()
	{
		return ActiveTexture;
	}

	public void Repaint(ref RenderTexture rtToPaint, Texture2D primaryRamp, Texture2D secondaryRamp)
	{
		if (null == ActiveTexture)
		{
			if (Application.isEditor)
			{
				PFLog.TechArt.Warning("Missing texture in one of the EE");
			}
		}
		else
		{
			if (!UseRamp1Mask && !UseRamp2Mask && !UseDefaultMask1 && !UseDefaultMask2)
			{
				return;
			}
			if (rtToPaint == null)
			{
				rtToPaint = RenderTexture.GetTemporary(m_Texture.width, m_Texture.height, 0, RenderTextureFormat.ARGB32);
				rtToPaint.name = m_Texture.name + "_RT";
			}
			if (s_RepaintMaterial == null)
			{
				s_RepaintMaterial = new Material(Shader.Find("Hidden/CharacterTextureRepaint"));
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = rtToPaint;
			try
			{
				Graphics.Blit(m_Texture, rtToPaint);
				Texture2D texture2D;
				if (UseRamp1Mask || UseDefaultMask1)
				{
					texture2D = ((!(null == Ramps.PrimaryRamp)) ? Ramps.PrimaryRamp : primaryRamp);
					if (texture2D != null)
					{
						if (UseDefaultMask1 && null != DefaultMask1)
						{
							s_RepaintMaterial.SetTexture(ShaderProps._Mask, DefaultMask1);
						}
						if (UseRamp1Mask && null != RampShadowTexture)
						{
							s_RepaintMaterial.SetTexture(ShaderProps._Mask, RampShadowTexture);
						}
						s_RepaintMaterial.SetFloat(ShaderProps._Specialmask, 1f);
						s_RepaintMaterial.SetTexture(ShaderProps._Ramp, texture2D);
						Graphics.Blit(m_Texture, rtToPaint, s_RepaintMaterial);
					}
				}
				if (!UseRamp2Mask)
				{
					return;
				}
				texture2D = ((!(null == Ramps.SecondaryRamp)) ? Ramps.SecondaryRamp : secondaryRamp);
				if (texture2D != null)
				{
					RenderTexture temporary = RenderTexture.GetTemporary(m_Texture.width, m_Texture.height, 0, RenderTextureFormat.ARGB32);
					Graphics.Blit(rtToPaint, temporary);
					if (UseDefaultMask2 && null != DefaultMask2)
					{
						s_RepaintMaterial.SetTexture(ShaderProps._Mask, DefaultMask2);
					}
					if (UseRamp2Mask && null != RampShadowTexture)
					{
						s_RepaintMaterial.SetTexture(ShaderProps._Mask, RampShadowTexture);
					}
					s_RepaintMaterial.SetFloat(ShaderProps._Specialmask, -1f);
					s_RepaintMaterial.SetTexture(ShaderProps._Ramp, texture2D);
					Graphics.Blit(m_Texture, temporary, s_RepaintMaterial);
					Graphics.Blit(temporary, rtToPaint);
					RenderTexture.ReleaseTemporary(temporary);
				}
			}
			finally
			{
				RenderTexture.active = active;
			}
		}
	}

	public string GetMainTextureName()
	{
		if ((bool)m_Texture)
		{
			return m_Texture.name;
		}
		return string.Empty;
	}

	public void DestroySourceAssets()
	{
		if (m_Texture != null)
		{
			UnityEngine.Object.DestroyImmediate(m_Texture, allowDestroyingAssets: true);
		}
	}
}
