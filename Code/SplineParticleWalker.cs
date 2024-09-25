using Dreamteck.Splines;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem), typeof(SplineComputer))]
public class SplineParticleWalker : MonoBehaviour
{
	[SerializeField]
	public bool isSetup;

	[SerializeField]
	private Vector3[] m_AllPossiblePositions;

	[SerializeField]
	private float _density = 1f;

	private SplineComputer m_SplineComputer;

	private ParticleSystem m_ParticleSystem;

	private ParticleSystem.Particle[] m_Particles;

	private float m_TotalLength;

	public float Density
	{
		get
		{
			return _density;
		}
		set
		{
			_density = value;
			Setup();
		}
	}

	private void OnEnable()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (!m_ParticleSystem)
		{
			m_ParticleSystem = GetComponent<ParticleSystem>();
		}
		if (!m_SplineComputer)
		{
			m_SplineComputer = GetComponent<SplineComputer>();
			m_SplineComputer.sampleMode = SplineComputer.SampleMode.Uniform;
		}
		if (m_AllPossiblePositions == null)
		{
			int num = (int)m_ParticleSystem.main.startLifetime.constant * 60;
			m_AllPossiblePositions = new Vector3[num];
			m_SplineComputer.sampleRate = num;
			isSetup = false;
		}
	}

	public void Setup()
	{
		using (ProfileScope.New("SplineParticleWalker.Setup"))
		{
			using (ProfileScope.New("Initialize"))
			{
				Initialize();
			}
			using (ProfileScope.New("GetTotalLength"))
			{
				GetTotalLength();
			}
			using (ProfileScope.New("GetAllPossiblePositions"))
			{
				GetParticlePositions();
			}
			ParticleSystem.EmissionModule emission = m_ParticleSystem.emission;
			emission.rateOverTime = _density * m_TotalLength;
			isSetup = true;
		}
	}

	public void GetTotalLength()
	{
		m_SplineComputer.sampleMode = SplineComputer.SampleMode.Uniform;
		m_TotalLength = m_SplineComputer.CalculateLength();
	}

	public void GetParticlePositions()
	{
		int num = (int)m_ParticleSystem.main.startLifetime.constant * 50;
		m_AllPossiblePositions = new Vector3[num];
		m_SplineComputer.sampleMode = SplineComputer.SampleMode.Uniform;
		int sampleRate = m_SplineComputer.sampleRate;
		m_SplineComputer.sampleRate = num;
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			float num3 = num2 * (float)i;
			m_AllPossiblePositions[i] = m_SplineComputer.EvaluatePosition(num3);
		}
		m_SplineComputer.sampleRate = sampleRate;
	}

	private void Update()
	{
		if (isSetup)
		{
			if (!m_ParticleSystem.isPlaying)
			{
				m_ParticleSystem.Play();
			}
			MoveParticleOptimized();
		}
		else
		{
			m_ParticleSystem.Pause();
		}
	}

	private void MoveParticleOptimized()
	{
		using (ProfileScope.New("SplineParticleWalker.MoveParticleOptimized"))
		{
			if (m_Particles == null)
			{
				m_Particles = new ParticleSystem.Particle[m_ParticleSystem.main.maxParticles];
			}
			m_ParticleSystem.GetParticles(m_Particles);
			for (int i = 0; i < m_ParticleSystem.particleCount; i++)
			{
				float num = (m_Particles[i].startLifetime - m_Particles[i].remainingLifetime) / m_Particles[i].startLifetime;
				int num2 = Mathf.CeilToInt((float)(m_AllPossiblePositions.Length - 1) * num);
				m_Particles[i].position = m_AllPossiblePositions[num2];
			}
			m_ParticleSystem.SetParticles(m_Particles, m_ParticleSystem.particleCount);
		}
	}
}
