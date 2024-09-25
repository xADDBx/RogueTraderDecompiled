using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public struct RenderTargetHandle
{
	public static readonly RenderTargetHandle CameraTarget = new RenderTargetHandle
	{
		Id = -1
	};

	public int Id { get; set; }

	public void Init(string shaderProperty)
	{
		Id = Shader.PropertyToID(shaderProperty);
	}

	public RenderTargetIdentifier Identifier()
	{
		if (Id == -1)
		{
			return BuiltinRenderTextureType.CameraTarget;
		}
		return new RenderTargetIdentifier(Id);
	}

	public bool Equals(RenderTargetHandle other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is RenderTargetHandle)
		{
			return Equals((RenderTargetHandle)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id;
	}

	public static bool operator ==(RenderTargetHandle c1, RenderTargetHandle c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(RenderTargetHandle c1, RenderTargetHandle c2)
	{
		return !c1.Equals(c2);
	}
}
