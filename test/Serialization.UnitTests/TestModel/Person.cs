namespace Serialization.UnitTests.TestModel
{
    public class Person : IPerson
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
    }

    public interface IPerson
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
    }
}
