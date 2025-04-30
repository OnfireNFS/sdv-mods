namespace CompanionAdventures.Framework;

public partial class Store
{
    private Test? _test = null;

    public void _Test(Test test)
    {
        _test ??= test;
    }
    
    public Test UseTest()
    {
        if (_test == null)
            Test.CreateStore(this);
        
        return _test!;
    }
}

public class Test
{
    private Test() { }
    public static void CreateStore(Store store)
    {
        store._Test(new Test());
    }
}