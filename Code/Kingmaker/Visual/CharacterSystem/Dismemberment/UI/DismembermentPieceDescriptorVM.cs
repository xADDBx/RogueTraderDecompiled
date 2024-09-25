using System;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class DismembermentPieceDescriptorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public Action<Vector3> ImpulseChanged;

	public DismembermentPieceDescriptor DismembermentPieceDescriptor { get; private set; }

	public DismembermentPieceDescriptorVM(DismembermentPieceDescriptor descriptor)
	{
		DismembermentPieceDescriptor = descriptor;
	}

	protected override void DisposeImplementation()
	{
	}

	internal void OnImpulseXChanged(float value)
	{
		DismembermentPieceDescriptor.Impulse.x = value;
		DismembermentPieceDescriptor.Impulse.Normalize();
		ImpulseChanged?.Invoke(DismembermentPieceDescriptor.Impulse);
	}

	internal void OnImpulseYChanged(float value)
	{
		DismembermentPieceDescriptor.Impulse.y = value;
		DismembermentPieceDescriptor.Impulse.Normalize();
		ImpulseChanged?.Invoke(DismembermentPieceDescriptor.Impulse);
	}

	internal void OnImpulseZChanged(float value)
	{
		DismembermentPieceDescriptor.Impulse.z = value;
		DismembermentPieceDescriptor.Impulse.Normalize();
		ImpulseChanged?.Invoke(DismembermentPieceDescriptor.Impulse);
	}

	internal void OnImpulseScaleMinChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			DismembermentPieceDescriptor.ImpulseMultiplier.x = result;
		}
		else if (string.IsNullOrEmpty(value))
		{
			DismembermentPieceDescriptor.ImpulseMultiplier.x = 0f;
		}
	}

	internal void OnImpulseScaleMaxChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			DismembermentPieceDescriptor.ImpulseMultiplier.y = result;
		}
		else if (string.IsNullOrEmpty(value))
		{
			DismembermentPieceDescriptor.ImpulseMultiplier.y = 0f;
		}
	}

	internal void OnIncomingImpulseScaleMinChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			DismembermentPieceDescriptor.IncomingImpulseMultiplier.x = result;
		}
		else if (string.IsNullOrEmpty(value))
		{
			DismembermentPieceDescriptor.IncomingImpulseMultiplier.x = result;
		}
	}

	internal void OnIncomingImpulseScaleMaxChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			DismembermentPieceDescriptor.IncomingImpulseMultiplier.y = result;
		}
		else if (string.IsNullOrEmpty(value))
		{
			DismembermentPieceDescriptor.IncomingImpulseMultiplier.y = result;
		}
	}

	internal void OnChildrenImpulseScaleMinChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			DismembermentPieceDescriptor.ChildrenImpulseMultiplier.x = result;
		}
		else if (string.IsNullOrEmpty(value))
		{
			DismembermentPieceDescriptor.ChildrenImpulseMultiplier.x = result;
		}
	}

	internal void OnChildrenImpulseScaleMaxChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			DismembermentPieceDescriptor.ChildrenImpulseMultiplier.y = result;
		}
		else if (string.IsNullOrEmpty(value))
		{
			DismembermentPieceDescriptor.ChildrenImpulseMultiplier.y = result;
		}
	}
}
