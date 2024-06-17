using System;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Fx;

[Serializable]
public class CastGroupForSpellSource
{
	[AssetPicker("")]
	[SerializeField]
	private GameObject m_PreCast;

	[AssetPicker("")]
	[SerializeField]
	private GameObject m_PreCastGround;

	[AssetPicker("")]
	[SerializeField]
	private GameObject m_Cast;

	[AssetPicker("")]
	[SerializeField]
	private GameObject m_CastGround;

	[AssetPicker("")]
	[SerializeField]
	private GameObject m_CastFail;

	[AssetPicker("")]
	[SerializeField]
	private GameObject m_CastFailGround;

	public GameObject PreCast => m_PreCast;

	public GameObject PreCastGround => m_PreCastGround;

	public GameObject Cast => m_Cast;

	public GameObject CastGround => m_CastGround;

	public GameObject CastFail => m_CastFail;

	public GameObject CastFailGround => m_CastFailGround;
}
