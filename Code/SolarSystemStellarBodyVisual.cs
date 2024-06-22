using System;
using System.Collections.Generic;
using Dreamteck.Splines;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;

[ExecuteInEditMode]
public class SolarSystemStellarBodyVisual : MonoBehaviour
{
	[Serializable]
	public class StellarOrbit
	{
		public Transform PositionForObjectOnOrbit;

		public Transform OrbitAxis;

		public SolarSystemStellarBodyVisual SettledStellarBody;
	}

	private float m_RotationSpeed = 3f;

	public bool NoVisualOrbit;

	public int OrbitLevel = -1;

	[Range(0f, 3f)]
	public int SecondaryOrbitsNumber;

	public bool overrideSecondaryOrbitAngles;

	public float SecondaryOrbitsDeviationZmin = -30f;

	public float SecondaryOrbitsDeviationZmax = 30f;

	public float SecondaryOrbitsDeviationXmin = -45f;

	public float SecondaryOrbitsDeviationXmax;

	[Space(20f)]
	public GameObject Visual;

	public GameObject Mechanics;

	[InspectorReadOnly]
	public LineRenderer OrbitLineRenderer;

	[InspectorReadOnly]
	public SplineComputer OrbitSplineComputer;

	[InspectorReadOnly]
	public GameObject ShadowProjection;

	[InspectorReadOnly]
	public LineRenderer SelectorMarkerRing;

	[InspectorReadOnly]
	public List<LineRenderer> SecondaryOrbits = new List<LineRenderer>();

	[InspectorReadOnly]
	public List<GameObject> SecondaryOrbitsArrows;

	[InspectorReadOnly]
	public List<GameObject> SelectorMarkerRingArrows = new List<GameObject>();

	[HideInInspector]
	public List<StellarOrbit> Orbits = new List<StellarOrbit>();

	private void Start()
	{
		if (Application.isPlaying)
		{
			if (OrbitLineRenderer != null)
			{
				OrbitLineRenderer.gameObject.EnsureComponent<HighlighterBlocker>();
			}
			if (OrbitSplineComputer != null)
			{
				OrbitSplineComputer.gameObject.EnsureComponent<HighlighterBlocker>();
			}
			if (ShadowProjection != null)
			{
				ShadowProjection.gameObject.EnsureComponent<HighlighterBlocker>();
			}
		}
	}

	private void Awake()
	{
		EditorInit();
	}

	private void OnEnable()
	{
		EditorInit();
	}

	private void EditorInit()
	{
	}

	private void Update()
	{
		if (Application.isPlaying && (bool)Visual)
		{
			Visual.transform.RotateAround(Visual.transform.position, Vector3.up, m_RotationSpeed * Time.deltaTime);
			Visual.transform.localPosition = Vector3.zero;
		}
	}
}
