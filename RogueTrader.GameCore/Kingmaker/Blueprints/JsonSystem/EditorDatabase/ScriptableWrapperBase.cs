using Kingmaker.Blueprints.Base;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase;

public abstract class ScriptableWrapperBase<T> : ScriptableWrapperBase, ISerializationCallbackReceiver, ICanBeMarkedDirty where T : class
{
	private string m_LastJson;

	private bool m_CheckForChanges;

	public abstract T WrappedInstance { get; set; }

	public override IHavePrototype PrototypeableInstance => WrappedInstance as IHavePrototype;

	public abstract T GetCanonicalInstance();

	protected abstract ScriptableWrapperBase<T> GetWrapper(T t);

	protected abstract string GetRootProperty();

	protected virtual void HandleAdditionalSync()
	{
	}

	protected virtual void CombineComponentsWithProto()
	{
	}

	public sealed override void SetBlueprintDirty()
	{
	}

	public void MarkDirty()
	{
		SetBlueprintDirty();
	}

	public void OnBeforeSerialize()
	{
		m_LastJson = m_LastJson ?? JsonUtility.ToJson(WrappedInstance);
	}

	public void OnAfterDeserialize()
	{
	}

	public sealed override void SyncPropertiesWithProto()
	{
	}

	public void UpdateOverridesList()
	{
	}
}
