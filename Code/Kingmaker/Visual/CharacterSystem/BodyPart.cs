using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
public class BodyPart
{
	private SkinnedMeshRenderer m_SkinnedRenderer;

	[HideIf("True")]
	[SerializeField]
	[LongAsEnum(typeof(BodyPartType))]
	private long m_Type;

	[DrawPreview]
	[HideIf("True")]
	public GameObject RendererPrefab;

	[HideIf("True")]
	public Material Material;

	[HideIf("True")]
	public List<CharacterTextureDescription> Textures;

	private bool True => true;

	public BodyPartType Type
	{
		get
		{
			return (BodyPartType)m_Type;
		}
		set
		{
			m_Type = (long)value;
		}
	}

	public SkinnedMeshRenderer SkinnedRenderer
	{
		get
		{
			if (RendererPrefab == null)
			{
				return null;
			}
			if (m_SkinnedRenderer == null || m_SkinnedRenderer.gameObject != RendererPrefab.gameObject)
			{
				m_SkinnedRenderer = RendererPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			return m_SkinnedRenderer;
		}
	}

	public static string GetPrefix(BodyPartType Type)
	{
		if (Type <= BodyPartType.UpperArms)
		{
			if (Type <= BodyPartType.KneeCops)
			{
				if (Type <= BodyPartType.Forearms)
				{
					if (Type <= BodyPartType.Eyes)
					{
						BodyPartType num = Type - 1;
						if ((ulong)num <= 3uL)
						{
							switch (num)
							{
							case (BodyPartType)0L:
								return "BT";
							case BodyPartType.Belt:
								return "BW";
							case BodyPartType.Belt | BodyPartType.Brows:
								return "CF";
							case BodyPartType.Brows:
								goto IL_0361;
							}
						}
						if (Type == BodyPartType.Eyes)
						{
							return "EY";
						}
					}
					else
					{
						switch (Type)
						{
						case BodyPartType.Feet:
							return "FT";
						case BodyPartType.Forearms:
							return "FA";
						}
					}
				}
				else
				{
					switch (Type)
					{
					case BodyPartType.Hands:
						return "HN";
					case BodyPartType.Head:
						return "HD";
					case BodyPartType.Helmet:
						return "HH";
					case BodyPartType.KneeCops:
						return "KC";
					}
				}
			}
			else
			{
				switch (Type)
				{
				case BodyPartType.LowerLegs:
					return "LL";
				case BodyPartType.MaskBottom:
					return "MA";
				case BodyPartType.Goggles:
					return "GG";
				case BodyPartType.RingLeft:
					return "RL";
				case BodyPartType.RingRight:
					return "RR";
				case BodyPartType.Skirt:
					return "SK";
				case BodyPartType.Spaulders:
					return "SP";
				case BodyPartType.Torso:
					return "TS";
				case BodyPartType.UpperArms:
					return "UA";
				}
			}
		}
		else
		{
			switch (Type)
			{
			case BodyPartType.UpperLegs:
				return "UL";
			case BodyPartType.MaskTop:
				return "MA";
			case BodyPartType.LowerLegsExtra:
				return "LE";
			case BodyPartType.TorsoExtra:
				return "TE";
			case BodyPartType.FacialHair:
				return "H2";
			case BodyPartType.HighCollar:
				return "CL";
			case BodyPartType.Lashes:
				return "LS";
			case BodyPartType.LowerArmsExtra:
				return "AE";
			case BodyPartType.Ears:
				return "EA";
			case BodyPartType.HeadBottom:
				return "HB";
			case BodyPartType.SpaulderL:
				return "SPL";
			case BodyPartType.SpaulderR:
				return "SPR";
			case BodyPartType.CuffL:
				return "CFL";
			case BodyPartType.CuffR:
				return "CFR";
			case BodyPartType.Hoses:
				return "HS";
			case BodyPartType.Teeth:
				return "TT";
			case BodyPartType.Augment1:
				return "AG1";
			}
		}
		goto IL_0361;
		IL_0361:
		return "BT";
	}
}
