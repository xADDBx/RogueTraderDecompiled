namespace Microsoft.Cci.Pdb;

internal interface IUsedNamespace
{
	IName Alias { get; }

	IName NamespaceName { get; }
}
