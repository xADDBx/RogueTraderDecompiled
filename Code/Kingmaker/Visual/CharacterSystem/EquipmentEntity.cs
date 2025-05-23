using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.DLC;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.CharacterSystem;

public class EquipmentEntity : ScriptableObject, IResource
{
	public interface IColorRampIndicesProvider
	{
		int PrimaryIndex { get; }

		int SecondaryIndex { get; }
	}

	public enum OutfitPartSpecialType
	{
		None,
		Backpack,
		Cloak,
		CloakSquashed
	}

	[Serializable]
	public class OutfitPart
	{
		[SerializeField]
		[Tooltip("Prefab to spawn. DO NOT USE FBX OR OTHER MESH HERE. ONLY PREFAB.")]
		public GameObject m_Prefab;

		[SerializeField]
		[Tooltip("Material to apply on spawn. If empty - material from prefab will be used.")]
		private Material m_Material;

		[HideInInspector]
		public Material tempMaterial;

		[SerializeField]
		[Tooltip("Local position which will be set on spawn")]
		public Vector3 m_Position;

		[SerializeField]
		[Tooltip("Local rotation which will be set on spawn")]
		public Vector3 m_Rotation;

		[SerializeField]
		[Tooltip("Local scale which will be set on spawn")]
		public Vector3 m_Scale;

		[FormerlySerializedAs("FirstCapeColorMask")]
		[Tooltip("R - first mask, G - second mask. B and A not in use.")]
		public Texture2D ColorMask;

		[SerializeField]
		[Tooltip("Will attach itself to this bone in character bones hierarchy")]
		private string m_BoneName;

		[SerializeField]
		[Tooltip("Is this outfit part must be visible in peaceful location (social hubs for example)")]
		private bool m_StaysInPeacefulMode;

		[SerializeField]
		[Tooltip("Is this outfit part must be visible only in doll rooms (inventory, character generation, etc)")]
		private bool m_OnlyInDollRoom;

		[SerializeField]
		private OutfitPartSpecialType m_Special;

		[SerializeField]
		[LongAsEnum(typeof(BodyPartType))]
		public long OutfitBodyPart;

		public Material material
		{
			get
			{
				return m_Material;
			}
			set
			{
				m_Material = value;
			}
		}

		public bool StaysInPeacefulMode => m_StaysInPeacefulMode;

		public OutfitPartSpecialType Special => m_Special;

		public bool OnlyInDollRoom => m_OnlyInDollRoom;

		public void SetMaterial(Material mat)
		{
			m_Material = mat;
		}

		public Renderer[] SetupMaterialsInNewOutfitPart(GameObject newOutfitPart, Material materialForPart)
		{
			Renderer[] componentsInChildren = newOutfitPart.GetComponentsInChildren<Renderer>();
			if (componentsInChildren.Length == 0)
			{
				return null;
			}
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sharedMaterial = m_Material;
			}
			return componentsInChildren;
		}

		public (GameObject outfit, Transform bone) Attach(Transform root, Dictionary<string, Transform> attachBonesCache)
		{
			Transform transform = ((!attachBonesCache.ContainsKey(m_BoneName)) ? root.FindChildRecursive(m_BoneName) : attachBonesCache[m_BoneName]);
			if (!transform)
			{
				PFLog.TechArt.Error("Can't find bone with name " + m_BoneName + " in transform " + root.name);
				return (outfit: null, bone: null);
			}
			if (!m_Prefab)
			{
				PFLog.TechArt.Error("No prefab link in equipment entity, nothing to Instantiate");
				return (outfit: null, bone: null);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_Prefab, transform, worldPositionStays: false);
			if ((bool)m_Material)
			{
				SetupMaterialsInNewOutfitPart(gameObject, m_Material);
			}
			gameObject.transform.localPosition = m_Position;
			gameObject.transform.localScale = m_Scale;
			gameObject.transform.localRotation = Quaternion.Euler(m_Rotation);
			return (outfit: gameObject, bone: transform);
		}

		public override string ToString()
		{
			return m_Prefab?.name + " on " + m_BoneName;
		}
	}

	public class PaintedTextures
	{
		private class PaintIndices
		{
			public int m_LastRepaintPrimary = -1;

			public int m_LastRepaintSecondary = -1;

			public int m_LastRepaintPrimarySpecial = -1;

			public int m_LastRepaintSecondarySpecial = -1;
		}

		private Dictionary<EquipmentEntity, PaintIndices> m_PaintIndices = new Dictionary<EquipmentEntity, PaintIndices>();

		private Dictionary<CharacterTextureDescription, RenderTexture> m_TexturesMap = new Dictionary<CharacterTextureDescription, RenderTexture>();

		public bool CheckNeedRepaint(EquipmentEntity ee, int primaryIndex, int secondaryIndex)
		{
			if (!m_PaintIndices.TryGetValue(ee, out var value))
			{
				value = new PaintIndices();
				m_PaintIndices.Add(ee, value);
			}
			bool result = value.m_LastRepaintPrimary != primaryIndex || value.m_LastRepaintSecondary != secondaryIndex;
			value.m_LastRepaintPrimary = primaryIndex;
			value.m_LastRepaintSecondary = secondaryIndex;
			return result;
		}

		public void Add(CharacterTextureDescription desc, RenderTexture rt)
		{
			m_TexturesMap[desc] = rt;
		}

		public RenderTexture Get(CharacterTextureDescription desc)
		{
			RenderTexture value = null;
			m_TexturesMap.TryGetValue(desc, out value);
			return value;
		}

		public void RemoveEquipmentEntity(EquipmentEntity ee)
		{
			foreach (BodyPart bodyPart in ee.BodyParts)
			{
				foreach (CharacterTextureDescription texture in bodyPart.Textures)
				{
					RenderTexture value = null;
					if (m_TexturesMap.TryGetValue(texture, out value) && value != null)
					{
						value.Release();
						UnityEngine.Object.DestroyImmediate(value);
					}
				}
			}
			m_PaintIndices.Remove(ee);
		}

		public void Clear()
		{
			foreach (KeyValuePair<CharacterTextureDescription, RenderTexture> item in m_TexturesMap)
			{
				if (!(item.Value == null))
				{
					RenderTexture.ReleaseTemporary(item.Value);
				}
			}
			m_TexturesMap.Clear();
			m_PaintIndices.Clear();
		}
	}

	private bool m_IsDirty;

	[CanBeNull]
	private Dictionary<string, Skeleton.Bone> m_BonesByName;

	[HideInInspector]
	public bool IsExportEnabled;

	[Tooltip("If this is checked, player won't be able to hide this part in DollRoom Settings. Works with helmets only")]
	public bool CantBeHiddenByDollRoom;

	[Tooltip("For showing helmet and other base outfit that players want to see even if they wear armor")]
	public bool ShowAboveAllIgnoreLayer;

	[SerializeField]
	private BlueprintDlcReference m_RequiresDlc;

	public int Layer;

	[LongAsEnumFlags(typeof(BodyPartType))]
	[SerializeField]
	[FormerlySerializedAs("HideBodyParts")]
	private long m_HideBodyParts;

	public bool ShowLowerMaterials;

	public List<Skeleton.Bone> SkeletonModifiers;

	[FormerlySerializedAs("ColorsProfile")]
	public CharacterColorsProfile PrimaryColorsProfile;

	public CharacterColorsProfile SecondaryColorsProfile;

	public CharacterBakedTextures BakedTextures;

	public Texture2D PreviewTexture;

	[SerializeField]
	public RampColorPreset ColorPresets;

	[SerializeField]
	[FormerlySerializedAs("PrimaryRamps")]
	private List<Texture2D> m_PrimaryRamps = new List<Texture2D>();

	[SerializeField]
	[FormerlySerializedAs("SecondaryRamps")]
	private List<Texture2D> m_SecondaryRamps = new List<Texture2D>();

	public List<BodyPart> BodyParts = new List<BodyPart>();

	public List<OutfitPart> OutfitParts = new List<OutfitPart>();

	[HideInInspector]
	public int ForcedPrimaryIndex = -1;

	[HideInInspector]
	public int ForcedSecondaryIndex = -1;

	public BlueprintDlc RequiresDlc
	{
		get
		{
			if (!m_RequiresDlc.IsEmpty())
			{
				return m_RequiresDlc.Get();
			}
			return null;
		}
	}

	public bool IsAvailable
	{
		get
		{
			if (!m_RequiresDlc.IsEmpty())
			{
				return m_RequiresDlc.Get().IsActive;
			}
			return true;
		}
	}

	public BodyPartType HideBodyParts
	{
		get
		{
			return (BodyPartType)m_HideBodyParts;
		}
		set
		{
			m_HideBodyParts = (long)value;
		}
	}

	public List<Texture2D> PrimaryRamps
	{
		get
		{
			if (PrimaryColorsProfile != null)
			{
				return PrimaryColorsProfile.Ramps;
			}
			return m_PrimaryRamps;
		}
		set
		{
			PrimaryColorsProfile.Ramps = value;
		}
	}

	public List<Texture2D> SecondaryRamps
	{
		get
		{
			if (SecondaryColorsProfile != null)
			{
				return SecondaryColorsProfile.Ramps;
			}
			return m_SecondaryRamps;
		}
		set
		{
			SecondaryColorsProfile.Ramps = value;
		}
	}

	public Dictionary<string, Skeleton.Bone> BonesByName
	{
		get
		{
			if (m_BonesByName == null)
			{
				m_BonesByName = new Dictionary<string, Skeleton.Bone>();
				foreach (Skeleton.Bone skeletonModifier in SkeletonModifiers)
				{
					m_BonesByName[skeletonModifier.Name] = skeletonModifier;
				}
			}
			return m_BonesByName;
		}
	}

	public void RepaintTextures(PaintedTextures paintedTextures, IColorRampIndicesProvider i)
	{
		RepaintTextures(paintedTextures, i?.PrimaryIndex ?? 0, i?.SecondaryIndex ?? 0);
	}

	public void RepaintTextures(PaintedTextures paintedTextures, int primaryRampIndex, int secondaryRampIndex)
	{
		if (!paintedTextures.CheckNeedRepaint(this, primaryRampIndex, secondaryRampIndex))
		{
			return;
		}
		Texture2D primaryRamp = null;
		if (primaryRampIndex >= 0 && primaryRampIndex < PrimaryRamps.Count)
		{
			primaryRamp = PrimaryRamps[primaryRampIndex];
		}
		Texture2D secondaryRamp = null;
		if (secondaryRampIndex >= 0 && secondaryRampIndex < SecondaryRamps.Count)
		{
			secondaryRamp = SecondaryRamps[secondaryRampIndex];
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			foreach (CharacterTextureDescription texture in bodyPart.Textures)
			{
				RenderTexture rtToPaint = paintedTextures.Get(texture);
				texture.Repaint(ref rtToPaint, primaryRamp, secondaryRamp);
				paintedTextures.Add(texture, rtToPaint);
			}
		}
	}

	public static void CreateAsset()
	{
		CustomAssetUtility.CreateAsset<EquipmentEntity>();
	}

	private static BodyPartType GetBodyPartType(string selectedGoName)
	{
		if (selectedGoName.Length < 3)
		{
			return BodyPartType.Head;
		}
		selectedGoName = selectedGoName.ToUpper().Remove(3);
		return selectedGoName switch
		{
			"_BT" => BodyPartType.Belt, 
			"_BW" => BodyPartType.Brows, 
			"_CF" => BodyPartType.Cuffs, 
			"_EY" => BodyPartType.Eyes, 
			"_FT" => BodyPartType.Feet, 
			"_FA" => BodyPartType.Forearms, 
			"_HN" => BodyPartType.Hands, 
			"_HD" => BodyPartType.Head, 
			"_HH" => BodyPartType.Helmet, 
			"_KC" => BodyPartType.KneeCops, 
			"_LL" => BodyPartType.LowerLegs, 
			"_MB" => BodyPartType.MaskBottom, 
			"_GG" => BodyPartType.Goggles, 
			"_RL" => BodyPartType.RingLeft, 
			"_RR" => BodyPartType.RingRight, 
			"_SK" => BodyPartType.Skirt, 
			"_SP" => BodyPartType.Spaulders, 
			"_TS" => BodyPartType.Torso, 
			"_UA" => BodyPartType.UpperArms, 
			"_UL" => BodyPartType.UpperLegs, 
			"_MA" => BodyPartType.MaskTop, 
			"_LE" => BodyPartType.LowerLegsExtra, 
			"_TE" => BodyPartType.TorsoExtra, 
			"_H2" => BodyPartType.FacialHair, 
			"_CL" => BodyPartType.HighCollar, 
			"_LS" => BodyPartType.Lashes, 
			"_AE" => BodyPartType.LowerArmsExtra, 
			"_EA" => BodyPartType.Ears, 
			"_HB" => BodyPartType.HeadBottom, 
			"_SPL" => BodyPartType.SpaulderL, 
			"_SPR" => BodyPartType.SpaulderR, 
			"_CFL" => BodyPartType.CuffL, 
			"_CFR" => BodyPartType.CuffR, 
			"_HS" => BodyPartType.Hoses, 
			"_TT" => BodyPartType.Teeth, 
			"_AG1" => BodyPartType.Augment1, 
			_ => BodyPartType.Head, 
		};
	}

	public bool IsDirty()
	{
		return m_IsDirty;
	}

	public void SetSkeletonDirty()
	{
		if (m_BonesByName != null)
		{
			m_BonesByName.Clear();
		}
		m_BonesByName = null;
		m_IsDirty = true;
	}

	public void ResetDirty()
	{
		m_IsDirty = false;
	}
}
