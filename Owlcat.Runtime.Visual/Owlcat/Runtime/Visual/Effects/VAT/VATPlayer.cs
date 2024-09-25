using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.VAT;

public class VATPlayer : MonoBehaviour
{
	[Serializable]
	public class JsonSettings
	{
		public SettingsEntry[] Data;
	}

	[Serializable]
	public class SettingsEntry
	{
		public float _doubleTex;

		public float _height;

		public float _normData;

		public float _numOfFrames;

		public float _packNorm;

		public float _packPscale;

		public float _padPowTwo;

		public float _paddedSizeX;

		public float _paddedSizeY;

		public float _pivMax;

		public float _pivMin;

		public float _posMax;

		public float _posMin;

		public float _scaleMax;

		public float _scaleMin;

		public float _speed;

		public float _textureSizeX;

		public float _textureSizeY;

		public float _width;
	}

	private Renderer m_Renderer;

	private List<MaterialPropertyBlock> m_Mpb = new List<MaterialPropertyBlock>();

	private float m_Time;

	private float m_DelayTimer;

	[SerializeField]
	private SettingsEntry m_HoudiniSettings;

	public float Speed = 1f;

	public bool IsLooped;

	public float Delay;

	public bool Pause { get; set; }

	public float FrameRate => FramesCount * m_HoudiniSettings._speed;

	public float FramesCount => m_HoudiniSettings._numOfFrames;

	public float Duration => 1f / m_HoudiniSettings._speed;

	public float CurrentFrame => m_Time / Duration * FramesCount;

	public bool IsReady
	{
		get
		{
			if (m_Renderer != null)
			{
				return m_HoudiniSettings != null;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		m_Time = 0f;
		m_DelayTimer = 0f;
		Pause = false;
		m_Renderer = GetComponent<Renderer>();
		if (m_Renderer != null)
		{
			m_Mpb.Clear();
			Material[] sharedMaterials = m_Renderer.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				_ = sharedMaterials[i];
				m_Mpb.Add(new MaterialPropertyBlock());
			}
		}
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null)
		{
			Bounds bounds = component.mesh.bounds;
			Bounds bounds2 = default(Bounds);
			bounds2.min = new Vector3(m_HoudiniSettings._posMin, m_HoudiniSettings._posMin, m_HoudiniSettings._posMin) - bounds.extents;
			bounds2.max = new Vector3(m_HoudiniSettings._posMax, m_HoudiniSettings._posMax, m_HoudiniSettings._posMax) + bounds.extents;
			component.mesh.bounds = bounds2;
		}
	}

	public void RewindToStart()
	{
		if (IsReady)
		{
			m_Time = 0f;
			InternalSetFrame(0f);
		}
	}

	public void RewindToEnd()
	{
		if (IsReady)
		{
			m_Time = FramesCount / FrameRate;
			InternalSetFrame(FramesCount);
		}
	}

	public void SetFrame(float currentFrame)
	{
		currentFrame = Mathf.Clamp(currentFrame, 0f, FramesCount);
		m_Time = currentFrame * Duration / FramesCount;
	}

	private void Update()
	{
		if (!IsReady || Pause)
		{
			return;
		}
		m_DelayTimer += Time.deltaTime;
		if (!(Delay > 0f) || !(m_DelayTimer < Delay))
		{
			float duration = Duration;
			float num = m_Time / duration * FramesCount;
			if (num < 0f)
			{
				num += FramesCount;
			}
			InternalSetFrame(num);
			m_Time += Time.deltaTime * Speed;
			m_Time = (IsLooped ? (m_Time % duration) : m_Time);
			if (!IsLooped && m_Time > duration)
			{
				Pause = true;
			}
		}
	}

	private void InternalSetFrame(float currentFrame)
	{
		for (int i = 0; i < m_Mpb.Count; i++)
		{
			MaterialPropertyBlock materialPropertyBlock = m_Mpb[i];
			materialPropertyBlock.SetFloat("_VatCurrentFrame", currentFrame);
			m_Renderer.SetPropertyBlock(materialPropertyBlock, i);
		}
	}
}
