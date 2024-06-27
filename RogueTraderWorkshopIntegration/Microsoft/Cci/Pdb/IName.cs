namespace Microsoft.Cci.Pdb;

internal interface IName
{
	int UniqueKey { get; }

	int UniqueKeyIgnoringCase { get; }

	string Value { get; }
}
