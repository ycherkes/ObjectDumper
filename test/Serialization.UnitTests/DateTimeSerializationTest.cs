using Newtonsoft.Json;
using System.Globalization;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class DateTimeSerializationTest
    {
        [Fact]
        public void SerializeDateOnlyCsharp()
        {
            var anonymous = new
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"var anonymousType = new 
{
    DateOnly = DateOnly.ParseExact(""2022-12-10"", ""O"")
};
", result);
        }

        [Fact]
        public void SerializeTimeOnlyCsharp()
        {
            var anonymous = new
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"var anonymousType = new 
{
    TimeOnly = TimeOnly.ParseExact(""22:55:33.1220000"", ""O"")
};
", result);
        }

        [Fact]
        public void SerializeDateOnlyVb()
        {
            var anonymous = new
            {
                DateOnly = DateOnly.ParseExact("2022-12-10", "O")
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"Dim anonymousType = New With {
    .DateOnly = DateOnly.ParseExact(""2022-12-10"", ""O"")
}
", result);
        }

        [Fact]
        public void SerializeTimeOnlyVb()
        {
            var anonymous = new
            {
                TimeOnly = TimeOnly.ParseExact("22:55:33.1220000", "O")
            };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                UseFullTypeName = false,
                DateTimeInstantiation = "Parse"
            }));

            Assert.Equal(
@"Dim anonymousType = New With {
    .TimeOnly = TimeOnly.ParseExact(""22:55:33.1220000"", ""O"")
}
", result);
        }

        [Fact]
        public void SerializeDateTimeXml()
        {
            var dateTime = DateTime.ParseExact("2023-07-16T23:55:00.6493620+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            var serializer = new XmlSerializer();

            var result = serializer.Serialize(dateTime, JsonConvert.SerializeObject(new
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            }));

            Assert.Equal(
                """
                <?xml version="1.0" encoding="utf-8"?>
                <DateTime>2023-07-16T21:55:00.649362Z</DateTime>
                """, result);
        }

        [Fact]
        public void SerializeDateTimePropertyXml()
        {
            var anonymous = new
            {
                DateTime = DateTime.ParseExact("2023-07-16T23:55:00.6493620+02:00", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
            
            var serializer = new XmlSerializer();

            var result = serializer.Serialize(anonymous, JsonConvert.SerializeObject(new
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            }));

            Assert.Equal(
                """
                <?xml version="1.0" encoding="utf-8"?>
                <__f__AnonymousType6OfDateTime>
                  <DateTime>2023-07-16T21:55:00.649362Z</DateTime>
                </__f__AnonymousType6OfDateTime>
                """, result);
        }
    }
}