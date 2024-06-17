using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class Body
{
	public Matrix4x4 LocalToWorld;

	public string Name;

	public List<Particle> Particles { get; private set; } = new List<Particle>();


	public List<Constraint> Constraints { get; private set; } = new List<Constraint>();


	public List<int2> DisconnectedConstraintsOffsetCount { get; private set; } = new List<int2>();


	public float Restitution { get; set; }

	public float Friction { get; set; }

	public float TeleportDistanceTreshold { get; set; }

	public Body(string name, List<Particle> particles, List<Constraint> constraints, List<int2> disconnectedConstraintsOffsetCount)
	{
		Name = name;
		Particles.AddRange(particles);
		Constraints.AddRange(constraints);
		if (disconnectedConstraintsOffsetCount != null)
		{
			DisconnectedConstraintsOffsetCount.AddRange(disconnectedConstraintsOffsetCount);
		}
		int hashCode = GetHashCode();
		for (int i = 0; i < Constraints.Count; i++)
		{
			Constraint value = Constraints[i];
			value.id = hashCode ^ i;
			Constraints[i] = value;
		}
	}

	public virtual void Dispose()
	{
	}

	public override string ToString()
	{
		return Name;
	}
}
