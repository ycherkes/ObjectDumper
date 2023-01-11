using Serialization.UnitTests.TestModel;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class DataTableSerializationTest
    {
        [Fact(Skip = "Skip")]
        public void SerializeDataTableCsharp()
        {
            var products = new DataTable
            {
                //TableName = "Table1",
                Columns = { { "Product", typeof(string) }, { "Lot", typeof(string) }, { "Qty", typeof(int) } },
                Rows =
                {
                    { "1", "2", 5 },
                    { "3", "4", 6 }
                }
            };
            var products1 = new DataTable
            {
                //TableName = "Table1",
                Columns = { { "Product", typeof(string) }, { "Lot", typeof(string) }, { "Qty", typeof(int) } },
                Rows =
                {
                    { "1", "2", 5 },
                    { "3", "4", 6 }
                }
            };

            products.PrimaryKey = new[] { products.Columns[0] };

            var bser = new BinaryFormatter();
            string res = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                bser.Serialize(ms, products);
                res = Encoding.UTF8.GetString(ms.ToArray());
            }

            var ser = new System.Xml.Serialization.XmlSerializer(typeof(DataTable));
            string resx = string.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                ser.Serialize(ms, products);
                resx = Encoding.UTF8.GetString(ms.ToArray());
            }
            var stringWriter = new StringWriter();
            //products.WriteXml(stringWriter, XmlWriteMode.WriteSchema);
            var res1 = stringWriter.ToString();

            var stringWriter1 = new StringWriter();
            var ser1 = new System.Xml.Serialization.XmlSerializer(typeof(List<Person>));
            ser1.Serialize(stringWriter1, new[] { new Person { FirstName = "TestFirstName" } }.ToList());
            var res2 = stringWriter1.ToString();

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(products, null);

            Assert.Equal(@"", result);
        }
    }
}