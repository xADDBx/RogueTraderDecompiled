using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Visual.Decals;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;
using Kingmaker.Visual.MaterialEffects.ColorTint;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using Kingmaker.Visual.Trails;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

namespace Kingmaker.Visual.Particles;

internal sealed class FxFader
{
	private sealed class EmissionEnabledMutator
	{
		private readonly List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();

		public void Add(ParticleSystem particleSystem)
		{
			if (particleSystem.emission.enabled)
			{
				m_ParticleSystems.Add(particleSystem);
			}
		}

		public void SetOpacity(float value)
		{
			foreach (ParticleSystem particleSystem in m_ParticleSystems)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				emission.enabled = value >= 1f;
			}
		}
	}

	private sealed class VfxFloatPropertyMutator
	{
		private struct Item
		{
			public VisualEffect VisualEffect;

			public float OriginalValue;
		}

		private readonly int m_PropertyId;

		private readonly float m_ZeroValue;

		private readonly List<Item> m_Items = new List<Item>();

		public VfxFloatPropertyMutator(int propertyId, float zeroValue)
		{
			m_PropertyId = propertyId;
			m_ZeroValue = zeroValue;
		}

		public void Add(VisualEffect visualEffect)
		{
			if (visualEffect.HasFloat(m_PropertyId))
			{
				m_Items.Add(new Item
				{
					VisualEffect = visualEffect,
					OriginalValue = visualEffect.GetFloat(m_PropertyId)
				});
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Item item in m_Items)
			{
				item.VisualEffect.SetFloat(m_PropertyId, Mathf.Lerp(m_ZeroValue, item.OriginalValue, value));
			}
		}
	}

	private sealed class VfxFloatFlagPropertyMutator
	{
		private struct Item
		{
			public VisualEffect VisualEffect;

			public float OriginalValue;
		}

		private readonly int m_PropertyId;

		private readonly List<Item> m_Items = new List<Item>();

		public VfxFloatFlagPropertyMutator(int propertyId)
		{
			m_PropertyId = propertyId;
		}

		public void Add(VisualEffect visualEffect)
		{
			if (visualEffect.HasFloat(m_PropertyId))
			{
				float @float = visualEffect.GetFloat(m_PropertyId);
				if (@float > 0f)
				{
					m_Items.Add(new Item
					{
						VisualEffect = visualEffect,
						OriginalValue = @float
					});
				}
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Item item in m_Items)
			{
				item.VisualEffect.SetFloat(m_PropertyId, (value >= 1f) ? item.OriginalValue : 0f);
			}
		}
	}

	private sealed class DecalMutator
	{
		private struct Item
		{
			public FxDecal Decal;

			public float OriginalValue;
		}

		private readonly List<Item> m_Items = new List<Item>();

		public void Add(FxDecal decal)
		{
			if ((bool)decal.SharedMaterial && decal.SharedMaterial.HasFloat(s_PropertyAlphaScale))
			{
				m_Items.Add(new Item
				{
					Decal = decal,
					OriginalValue = decal.SharedMaterial.GetFloat(s_PropertyAlphaScale)
				});
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Item item in m_Items)
			{
				item.Decal.MaterialProperties.SetFloat(s_PropertyAlphaScale, Mathf.Lerp(0f, item.OriginalValue, value));
			}
		}
	}

	private sealed class AnimationSetupMutator
	{
		public readonly List<ColorTintAnimationSetup> ColorTintItems = new List<ColorTintAnimationSetup>();

		public readonly List<RimLightingAnimationSetup> RimLightingItems = new List<RimLightingAnimationSetup>();

		public readonly List<DissolveSetup> DissolveItems = new List<DissolveSetup>();

		public readonly List<AdditionalAlbedoSetup> AdditionalAlbedoItems = new List<AdditionalAlbedoSetup>();

		public readonly List<AnimatedLight> AnimatedLightItems = new List<AnimatedLight>();

		public void SetFadeOutCorrection(float value)
		{
			foreach (ColorTintAnimationSetup colorTintItem in ColorTintItems)
			{
				colorTintItem.Settings.FadeOut = value;
			}
			foreach (RimLightingAnimationSetup rimLightingItem in RimLightingItems)
			{
				rimLightingItem.Settings.FadeOut = value;
			}
			foreach (DissolveSetup dissolveItem in DissolveItems)
			{
				dissolveItem.Settings.FadeOut = value;
			}
			foreach (AdditionalAlbedoSetup additionalAlbedoItem in AdditionalAlbedoItems)
			{
				additionalAlbedoItem.Settings.FadeOut = value;
			}
			foreach (AnimatedLight animatedLightItem in AnimatedLightItems)
			{
				animatedLightItem.FadeOut = value;
			}
		}
	}

	private sealed class DissolveKeywordMutator
	{
		private readonly List<Material> m_Items = new List<Material>();

		public void Add(Material material)
		{
			if (material.shader.keywordSpace.FindKeyword("DISSOLVE_ON").isValid && !material.IsKeywordEnabled("DISSOLVE_ON"))
			{
				m_Items.Add(material);
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Material item in m_Items)
			{
				if (value >= 1f)
				{
					item.DisableKeyword("DISSOLVE_ON");
				}
				else
				{
					item.EnableKeyword("DISSOLVE_ON");
				}
			}
		}
	}

	private sealed class FloatPropertyMutator
	{
		private struct Item
		{
			public Material Material;

			public int PropertyId;

			public float ZeroValue;

			public float OneValue;
		}

		private readonly List<Item> m_Items = new List<Item>();

		public void Add(Material material, int propertyId, float zeroValue)
		{
			if (material.HasFloat(propertyId))
			{
				m_Items.Add(new Item
				{
					Material = material,
					PropertyId = propertyId,
					ZeroValue = zeroValue,
					OneValue = material.GetFloat(propertyId)
				});
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Item item in m_Items)
			{
				item.Material.SetFloat(item.PropertyId, Mathf.Lerp(item.ZeroValue, item.OneValue, value));
			}
		}
	}

	private sealed class FloatFlagPropertyMutator
	{
		private struct Item
		{
			public Material Material;

			public int PropertyId;

			public float EnabledValue;
		}

		private readonly List<Item> m_Items = new List<Item>();

		public void Add(Material material, int propertyId)
		{
			if (material.HasFloat(propertyId))
			{
				float @float = material.GetFloat(propertyId);
				if (!Mathf.Approximately(@float, 0f))
				{
					m_Items.Add(new Item
					{
						Material = material,
						PropertyId = propertyId,
						EnabledValue = @float
					});
				}
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Item item in m_Items)
			{
				item.Material.SetFloat(item.PropertyId, (value >= 1f) ? item.EnabledValue : 0f);
			}
		}
	}

	private sealed class ColorAlphaPropertyMutator
	{
		private struct Item
		{
			public Material Material;

			public int PropertyId;

			public Color ZeroValue;

			public Color OneValue;
		}

		private readonly List<Item> m_Items = new List<Item>();

		public void Add(Material material, int propertyId, float zeroValue)
		{
			if (material.HasColor(propertyId))
			{
				Color color = material.GetColor(propertyId);
				m_Items.Add(new Item
				{
					Material = material,
					PropertyId = propertyId,
					ZeroValue = new Color(color.r, color.g, color.b, zeroValue),
					OneValue = color
				});
			}
		}

		public void SetOpacity(float value)
		{
			foreach (Item item in m_Items)
			{
				item.Material.SetColor(item.PropertyId, Color.Lerp(item.ZeroValue, item.OneValue, value));
			}
		}
	}

	private const string kKeywordDissolveOn = "DISSOLVE_ON";

	private static readonly int s_PropertyDissolve = Shader.PropertyToID("_Dissolve");

	private static readonly int s_PropertyDissolveWidth = Shader.PropertyToID("_DissolveWidth");

	private static readonly int s_PropertyAlphaScale = Shader.PropertyToID("_AlphaScale");

	private static readonly int s_PropertyBaseColor = Shader.PropertyToID("_BaseColor");

	private static readonly int s_PropertySystemOpacity = Shader.PropertyToID("System_Opacity");

	private static readonly int s_PropertySystemEmission = Shader.PropertyToID("System_Emission");

	private static readonly int s_PropertyDissolveMap = Shader.PropertyToID("_DissolveMap");

	private readonly EmissionEnabledMutator m_EmissionEnabledMutator = new EmissionEnabledMutator();

	private readonly AnimationSetupMutator m_AnimationSetupMutator = new AnimationSetupMutator();

	private readonly VfxFloatPropertyMutator m_SystemOpacityMutator = new VfxFloatPropertyMutator(s_PropertySystemOpacity, 0f);

	private readonly VfxFloatFlagPropertyMutator m_SystemEmissionMutator = new VfxFloatFlagPropertyMutator(s_PropertySystemEmission);

	private readonly DecalMutator m_DecalMutator = new DecalMutator();

	private readonly DissolveKeywordMutator m_DissolveKeywordMutator = new DissolveKeywordMutator();

	private readonly FloatPropertyMutator m_FloatPropertyMutator = new FloatPropertyMutator();

	private readonly ColorAlphaPropertyMutator m_ColorAlphaPropertyMutator = new ColorAlphaPropertyMutator();

	private readonly FloatFlagPropertyMutator m_FloatFlagPropertyMutator = new FloatFlagPropertyMutator();

	public FxFader(GameObject root, List<ParticleSystem> stopEmissionParticleSystems, FxFadeOut.CustomMaterialPropertyData[] customMaterialProperties, bool stopVisualEffectEmission)
	{
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer is ParticleSystemRenderer particleSystemRenderer)
			{
				ParticleSystem component = particleSystemRenderer.GetComponent<ParticleSystem>();
				if (stopEmissionParticleSystems == null || !stopEmissionParticleSystems.Contains(component))
				{
					Material sharedMaterial = particleSystemRenderer.sharedMaterial;
					if ((bool)sharedMaterial)
					{
						Material material2 = (particleSystemRenderer.material = CreateMaterialInstance(sharedMaterial));
						AddMaterial(material2, customMaterialProperties, includeCustomMaterials: true);
					}
					Material trailMaterial = particleSystemRenderer.trailMaterial;
					if ((bool)trailMaterial)
					{
						Material material4 = (particleSystemRenderer.trailMaterial = CreateMaterialInstance(trailMaterial));
						AddMaterial(material4, customMaterialProperties, includeCustomMaterials: true);
					}
				}
				else
				{
					m_EmissionEnabledMutator.Add(component);
				}
			}
			else
			{
				if (!(renderer is MeshRenderer meshRenderer))
				{
					continue;
				}
				List<Material> value;
				using (CollectionPool<List<Material>, Material>.Get(out value))
				{
					meshRenderer.GetSharedMaterials(value);
					for (int j = 0; j < value.Count; j++)
					{
						Material material6 = (value[j] = CreateMaterialInstance(value[j]));
						AddMaterial(material6, customMaterialProperties, includeCustomMaterials: false);
					}
					meshRenderer.SetSharedMaterialsExt(value);
				}
			}
		}
		List<FxDecal> value2;
		using (CollectionPool<List<FxDecal>, FxDecal>.Get(out value2))
		{
			root.GetComponentsInChildren(value2);
			foreach (FxDecal item in value2)
			{
				m_DecalMutator.Add(item);
			}
		}
		List<CompositeTrailRenderer> value3;
		using (CollectionPool<List<CompositeTrailRenderer>, CompositeTrailRenderer>.Get(out value3))
		{
			root.GetComponentsInChildren(value3);
			foreach (CompositeTrailRenderer item2 in value3)
			{
				if (item2.Material != null)
				{
					Material material7 = (item2.Material = CreateMaterialInstance(item2.Material));
					m_FloatPropertyMutator.Add(material7, s_PropertyAlphaScale, 0f);
				}
			}
		}
		List<VisualEffect> value4;
		using (CollectionPool<List<VisualEffect>, VisualEffect>.Get(out value4))
		{
			root.GetComponentsInChildren(value4);
			foreach (VisualEffect item3 in value4)
			{
				m_SystemOpacityMutator.Add(item3);
				if (stopVisualEffectEmission)
				{
					m_SystemEmissionMutator.Add(item3);
				}
			}
		}
		root.GetComponentsInChildren(m_AnimationSetupMutator.ColorTintItems);
		root.GetComponentsInChildren(m_AnimationSetupMutator.RimLightingItems);
		root.GetComponentsInChildren(m_AnimationSetupMutator.DissolveItems);
		root.GetComponentsInChildren(m_AnimationSetupMutator.AdditionalAlbedoItems);
		root.GetComponentsInChildren(m_AnimationSetupMutator.AnimatedLightItems);
	}

	private void AddMaterial(Material material, FxFadeOut.CustomMaterialPropertyData[] customMaterialProperties, bool includeCustomMaterials)
	{
		if (material == null)
		{
			return;
		}
		if (material.shader.name == "Owlcat/Particles")
		{
			m_FloatPropertyMutator.Add(material, s_PropertyAlphaScale, 0f);
			m_ColorAlphaPropertyMutator.Add(material, s_PropertyBaseColor, 0f);
		}
		else if (material.shader.name == "Owlcat/Lit")
		{
			if (material.HasTexture(s_PropertyDissolveMap) && !material.GetTexture(s_PropertyDissolveMap))
			{
				material.SetTexture(s_PropertyDissolveMap, BlueprintRoot.Instance.DefaultDissolveTexture);
			}
			m_FloatPropertyMutator.Add(material, s_PropertyDissolve, 1f);
			m_FloatFlagPropertyMutator.Add(material, s_PropertyDissolveWidth);
			m_DissolveKeywordMutator.Add(material);
		}
		else if (includeCustomMaterials && customMaterialProperties != null)
		{
			Span<FxFadeOut.CustomMaterialPropertyData> span = customMaterialProperties.AsSpan();
			for (int i = 0; i < span.Length; i++)
			{
				ref FxFadeOut.CustomMaterialPropertyData reference = ref span[i];
				m_FloatPropertyMutator.Add(material, reference.PropertyId, reference.FadeOutValue);
			}
		}
	}

	public void SetOpacity(float value, bool applyFadeOutCorrection)
	{
		m_EmissionEnabledMutator.SetOpacity(value);
		m_SystemOpacityMutator.SetOpacity(value);
		m_SystemEmissionMutator.SetOpacity(value);
		m_DecalMutator.SetOpacity(value);
		m_DissolveKeywordMutator.SetOpacity(value);
		m_FloatPropertyMutator.SetOpacity(value);
		m_ColorAlphaPropertyMutator.SetOpacity(value);
		m_FloatFlagPropertyMutator.SetOpacity(value);
		m_AnimationSetupMutator.SetFadeOutCorrection(applyFadeOutCorrection ? value : 1f);
	}

	private static Material CreateMaterialInstance(Material original)
	{
		if (original == null)
		{
			return null;
		}
		return new Material(original);
	}
}
