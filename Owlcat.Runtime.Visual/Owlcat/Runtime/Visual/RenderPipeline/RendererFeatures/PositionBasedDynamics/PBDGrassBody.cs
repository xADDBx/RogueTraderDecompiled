using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.IndirectRendering.Details;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics;

[RequireComponent(typeof(DetailsMesh))]
public class PBDGrassBody : PBDBodyBase<GrassBody>
{
	private DetailsMesh m_DetailsMesh;

	public float Mass = 1f;

	public float Radius = 0.1f;

	[Range(0f, 1f)]
	public float AngularStiffness = 0.1f;

	[Range(0f, 1f)]
	public float VelocityInfluence = 1f;

	public bool IsPBDBodyDataInitialized { get; private set; }

	protected override bool ValidateBeforeInitialize()
	{
		return true;
	}

	protected override void InitializeInternal()
	{
		InitBody();
	}

	protected override void Dispose()
	{
		PBD.UnregisterBody(m_Body);
		m_Body.Dispose();
		m_Body = null;
	}

	protected override void OnBodyDataUpdated()
	{
		IsPBDBodyDataInitialized = true;
	}

	private void InitBody()
	{
		m_DetailsMesh = GetComponent<DetailsMesh>();
		if (!(m_DetailsMesh.Data == null))
		{
			List<Particle> list = ListPool<Particle>.Claim();
			List<Constraint> list2 = ListPool<Constraint>.Claim();
			m_DetailsMesh.Data.SortInstancesByMortonCode();
			for (int i = 0; i < m_DetailsMesh.Data.Instances.Count; i++)
			{
				DetailInstanceData detailInstanceData = m_DetailsMesh.Data.Instances[i];
				float3 @float = detailInstanceData.Position + new Vector3(0f, m_DetailsMesh.Mesh.bounds.min.y, 0f);
				float3 float2 = @float + new float3(0f, m_DetailsMesh.Mesh.bounds.size.y, 0f) * detailInstanceData.Scale;
				Particle particle = default(Particle);
				particle.BasePosition = @float;
				particle.Position = @float;
				particle.Predicted = @float;
				particle.Mass = 0f;
				particle.Flags = 5u;
				particle.Radius = 0f;
				particle.Velocity = 0;
				Particle item = particle;
				particle = default(Particle);
				particle.BasePosition = float2;
				particle.Position = float2;
				particle.Predicted = float2;
				particle.Mass = Mass;
				particle.Flags = 1u;
				particle.Radius = Radius;
				particle.Velocity = 0;
				Particle item2 = particle;
				int count = list.Count;
				list.Add(item);
				list.Add(item2);
				Constraint constraint = default(Constraint);
				constraint.index0 = count;
				constraint.index1 = count + 1;
				constraint.type = ConstraintType.Grass;
				Constraint item3 = constraint;
				item3.SetDistanceAngularParameters(1f, 1f, AngularStiffness, VelocityInfluence, useParentChildDirection: false);
				list2.Add(item3);
			}
			base.Body = new GrassBody(base.name, list, list2);
			base.Body.Restitution = base.Restitution;
			base.Body.Friction = base.Friction;
			base.Body.TeleportDistanceTreshold = base.TeleportDistanceTreshold;
			PBD.RegisterBody(base.Body);
			ListPool<Particle>.Release(list);
			ListPool<Constraint>.Release(list2);
			IsPBDBodyDataInitialized = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
	}
}
