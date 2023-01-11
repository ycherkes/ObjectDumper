using Newtonsoft.Json;
using Serialization.UnitTests.TestModel;
using YellowFlavor.Serialization.Implementation;
using YellowFlavor.Serialization.Implementation.Settings;
using JsonSerializer = YellowFlavor.Serialization.Implementation.JsonSerializer;

namespace Serialization.UnitTests
{
    public class SerializationSettingsTest
    {
        [Fact]
        public void JsonSerializeSnakeCaseNamingStrategyTest()
        {
            var person = new Person
            {
                FirstName = "Boris",
                Age = 54
            };

            var serializer = new JsonSerializer();

            var result = serializer.Serialize(person, JsonConvert.SerializeObject(new JsonSettings
            {
                NamingStrategy = "SnakeCase"
            }));

            Assert.Equal(
@"{
  ""first_name"": ""Boris"",
  ""age"": 54
}",
result);
        }

        [Fact]
        public void YamlSerializeUnderscoredNamingConventionTest()
        {
            var person = new Person
            {
                FirstName = "Boris",
                Age = 54
            };

            var serializer = new YamlSerializer();

            var result = serializer.Serialize(person, JsonConvert.SerializeObject(new YamlSettings
            {
                NamingConvention = "Underscored"
            }));

            Assert.Equal(
@"first_name: Boris
age: 54
",
result);
        }
    }
}
