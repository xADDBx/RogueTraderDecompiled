using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;
using Kingmaker.Visual.MaterialEffects.ColorTint;
using Kingmaker.Visual.MaterialEffects.CustomMaterialProperty;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.MaterialEffects.LayeredMaterial;
using Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.MaterialEffects;

public class StandardMaterialController : UpdateableBehaviour
{
	[Flags]
	private enum DirtyFlags
	{
		None = 0,
		RenderersAndMaterials = 1,
		MaterialProperties = 2
	}

	[SerializeField]
	private ColorTintAnimationController m_ColorTintController = new ColorTintAnimationController();

	[SerializeField]
	private RimLightingAnimationController m_RimController = new RimLightingAnimationController();

	[SerializeField]
	private DissolveAnimationController m_DissolveController = new DissolveAnimationController();

	[SerializeField]
	[FormerlySerializedAs("m_PetrificationController")]
	private AdditionalAlbedoAnimationController m_AdditionalAlbedoController = new AdditionalAlbedoAnimationController();

	[SerializeField]
	private MaterialParametersOverrideController m_MaterialParametersOverrideController = new MaterialParametersOverrideController();

	private CustomMaterialPropertyAnimationController m_CustomMaterialPropertyAnimationController = new CustomMaterialPropertyAnimationController();

	private LayeredMaterialController m_OverlayMaterialController;

	private DirtyFlags m_DirtyFlags;

	internal ColorTintAnimationController ColorTintController => m_ColorTintController;

	internal RimLightingAnimationController RimController => m_RimController;

	internal DissolveAnimationController DissolveController => m_DissolveController;

	internal AdditionalAlbedoAnimationController AdditionalAlbedoController => m_AdditionalAlbedoController;

	internal MaterialParametersOverrideController MaterialParametersOverrideController => m_MaterialParametersOverrideController;

	internal CustomMaterialPropertyAnimationController CustomMaterialPropertyAnimationController => m_CustomMaterialPropertyAnimationController;

	[UsedImplicitly]
	private void Awake()
	{
		Setup();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		m_CustomMaterialPropertyAnimationController.Dispose();
	}

	private void Setup()
	{
		SetupMaterials();
		SetupControllers();
		SetupOverlayController();
	}

	private void Cleanup()
	{
		CleanupOverlayController();
		CleanupMaterials();
	}

	private void CleanupMaterials()
	{
		m_ColorTintController.ClearMaterials();
		m_RimController.ClearMaterials();
		m_DissolveController.ClearMaterials();
		m_AdditionalAlbedoController.ClearMaterials();
		m_MaterialParametersOverrideController.ClearMaterials();
	}

	private void SetupMaterials()
	{
		List<Renderer> value;
		using (CollectionPool<List<Renderer>, Renderer>.Get(out value))
		{
			GetComponentsInChildren(value);
			foreach (Renderer item in value)
			{
				if (!(item is MeshRenderer) && !(item is SkinnedMeshRenderer) && !(item is ParticleSystemRenderer) && !(item is LineRenderer) && !(item is TrailRenderer))
				{
					continue;
				}
				List<Material> value2;
				using (CollectionPool<List<Material>, Material>.Get(out value2))
				{
					bool flag = false;
					item.GetSharedMaterials(value2);
					int i = 0;
					for (int count = value2.Count; i < count; i++)
					{
						Material material = value2[i];
						if (!(material == null) && !(material.shader == null))
						{
							bool flag2 = ColorTintMaterial.IsMaterialCompatible(material);
							bool flag3 = RimLightingMaterial.IsMaterialCompatible(material);
							bool flag4 = DissolveMaterial.IsMaterialCompatible(material);
							bool flag5 = AdditionalAlbedoMaterial.IsMaterialCompatible(material);
							bool flag6 = ParametersOverrideMaterial.IsMaterialCompatible(material);
							Material material3 = (value2[i] = new Material(material));
							if (flag2)
							{
								ColorTintAnimationController colorTintController = m_ColorTintController;
								ColorTintMaterial material4 = new ColorTintMaterial(material3);
								colorTintController.AddMaterial(in material4);
							}
							if (flag3)
							{
								m_RimController.AddMaterial(new RimLightingMaterial(material3));
							}
							if (flag4)
							{
								m_DissolveController.AddMaterial(new DissolveMaterial(material3));
							}
							if (flag5)
							{
								AdditionalAlbedoAnimationController additionalAlbedoController = m_AdditionalAlbedoController;
								AdditionalAlbedoMaterial material5 = new AdditionalAlbedoMaterial(material3);
								additionalAlbedoController.AddMaterial(in material5);
							}
							if (flag6)
							{
								m_MaterialParametersOverrideController.AddMaterial(new ParametersOverrideMaterial(material3));
							}
							m_CustomMaterialPropertyAnimationController.AddMaterial(material3);
							flag = true;
						}
					}
					if (flag)
					{
						item.SetSharedMaterialsExt(value2);
					}
				}
			}
		}
	}

	protected override void OnEnabled()
	{
		DoUpdate();
		Character component = GetComponent<Character>();
		if (component != null)
		{
			component.OnUpdated += OnCharacterUpdated;
		}
	}

	protected override void OnDisabled()
	{
		Character component = GetComponent<Character>();
		if (component != null)
		{
			component.OnUpdated -= OnCharacterUpdated;
		}
	}

	public void InvalidateRenderersAndMaterials()
	{
		m_DirtyFlags |= DirtyFlags.RenderersAndMaterials;
	}

	public void InvalidateMaterialsTextures()
	{
		m_DirtyFlags |= DirtyFlags.MaterialProperties;
	}

	private void OnCharacterUpdated(Character character)
	{
		InvalidateRenderersAndMaterials();
	}

	private void ReinitRenderersAndMaterials()
	{
		m_ColorTintController.RevertToDefaults();
		m_RimController.RevertToDefaults();
		m_DissolveController.RevertToDefaults();
		m_AdditionalAlbedoController.RevertToDefaults();
		m_MaterialParametersOverrideController.RevertToDefaults();
		m_CustomMaterialPropertyAnimationController.ClearMaterials();
		Cleanup();
		Setup();
	}

	private void UpdateMaterialProperties()
	{
		m_DissolveController.UpdateMaterialProperties();
		m_AdditionalAlbedoController.UpdateMaterialProperties();
		m_MaterialParametersOverrideController.UpdateMaterialProperties();
		m_OverlayMaterialController.RefreshMaterialPropertiesSnapshots();
	}

	public override void DoUpdate()
	{
		if (m_DirtyFlags != 0)
		{
			if ((m_DirtyFlags & DirtyFlags.MaterialProperties) != 0)
			{
				UpdateMaterialProperties();
			}
			if ((m_DirtyFlags & DirtyFlags.RenderersAndMaterials) != 0)
			{
				ReinitRenderersAndMaterials();
			}
			m_DirtyFlags = DirtyFlags.None;
		}
		using (Counters.StandardMaterialController?.Measure())
		{
			m_ColorTintController.Update();
			m_RimController.Update();
			m_DissolveController.Update();
			m_AdditionalAlbedoController.Update();
			m_MaterialParametersOverrideController.Update();
			m_OverlayMaterialController.Update();
			m_CustomMaterialPropertyAnimationController.Update();
		}
	}

	private void SetupOverlayController()
	{
		if (m_OverlayMaterialController == null)
		{
			m_OverlayMaterialController = new LayeredMaterialController(base.gameObject, new MaterialPropertyBlock());
		}
		UnitAnimationManager componentInChildren = GetComponentInChildren<UnitAnimationManager>();
		StarshipView componentInChildren2 = GetComponentInChildren<StarshipView>();
		GameObject gameObject = ((componentInChildren != null) ? componentInChildren.gameObject : ((!(componentInChildren2 != null)) ? base.gameObject : componentInChildren2.gameObject));
		m_OverlayMaterialController.SetMaxActiveLayersCount(BlueprintRoot.Instance.FxRoot.MaxMaterialLayersCount);
		List<Renderer> value;
		using (CollectionPool<List<Renderer>, Renderer>.Get(out value))
		{
			gameObject.GetComponentsInChildren(includeInactive: true, value);
			m_OverlayMaterialController.SetMaxActiveLayersCount(BlueprintRoot.Instance.FxRoot.MaxMaterialLayersCount);
			m_OverlayMaterialController.RefreshScriptPropertiesSnapshot();
			m_OverlayMaterialController.SetupRenderers(value);
		}
	}

	private void CleanupOverlayController()
	{
		if (m_OverlayMaterialController != null)
		{
			m_OverlayMaterialController.CleanupRenderers();
		}
	}

	private void SetupControllers()
	{
		m_DissolveController.InvalidateCache();
	}

	internal bool TryAddOverlayAnimation(LayeredMaterialAnimationSetup setup, out int token)
	{
		return m_OverlayMaterialController.TryAddAnimation(setup, out token);
	}

	internal void RemoveOverlayAnimation(int token)
	{
		m_OverlayMaterialController.RemoveAnimation(token);
	}
}
