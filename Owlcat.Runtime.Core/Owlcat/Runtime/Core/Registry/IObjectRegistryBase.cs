namespace Owlcat.Runtime.Core.Registry;

internal interface IObjectRegistryBase
{
	void Register(object obj);

	void Delete(object obj);

	void TestClearStaticInstance();

	ObjectRegistryEnumerator<T2> GetEnumerator<T2>();
}
