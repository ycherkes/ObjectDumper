using System.Collections.Immutable;
using YellowFlavor.Serialization.Implementation;

namespace Serialization.UnitTests
{
    public class ArraySerializationTest
    {
        [Fact]
        public void SerializeArrayOfArraysCsharp()
        {
            int[][] array = { new[] { 1 } };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"var arrayOfArrayOfInt = new int[][]
{
    new int[]
    {
        1
    }
};
", result);
        }

        [Fact]
        public void SerializeImmutableArrayOfArraysCsharp()
        {
            var array = new[] { new[] { 1 } }.ToImmutableArray();

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"var immutableArrayOfArrayOfInt = new int[][]
{
    new int[]
    {
        1
    }
}.ToImmutableArray();
", result);
        }

        [Fact]
        public void Serialize2DimensionalArrayCsharp()
        {
            var array = new[,] { { 2, 3, 4 }, { 5, 6, 7 } };
            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"var arrayOfInt = new int[,]
{
    {
        2,
        3,
        4
    },
    {
        5,
        6,
        7
    }
};
", result);
        }

        [Fact]
        public void Serialize2DimensionalAnonymousArrayCsharp()
        {
            var array = new[,] { { new { Name = "Test1" } }, { new { Name = "Test2" } } };
            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"var arrayOfAnonymousType = new [,]
{
    {
        new 
        {
            Name = ""Test1""
        }
    },
    {
        new 
        {
            Name = ""Test2""
        }
    }
};
", result);
        }

        [Fact]
        public void SerializeArrayOfArraysAnonymousCsharp()
        {
            var array = new[] { new[] { new { Name = "Clark" } } };

            var serializer = new CSharpSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"var arrayOfArrayOfAnonymousType = new []
{
    new []
    {
        new 
        {
            Name = ""Clark""
        }
    }
};
", result);
        }

        [Fact]
        public void SerializeArrayOfArraysVb()
        {
            int[][] array = { new[] { 1 } };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"Dim arrayOfArrayOfInteger = New Integer()(){
    New Integer(){
        1
    }
}
", result);
        }

        [Fact]
        public void Serialize2DimensionalArrayVb()
        {
            var array = new[,] { { 2, 3, 4 }, { 5, 6, 7 } };
            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"Dim arrayOfInteger = New Integer(,){
    {
        2,
        3,
        4
    },
    {
        5,
        6,
        7
    }
}
", result);
        }

        [Fact]
        public void Serialize2DimensionalAnonymousArrayVb()
        {
            var array = new[,] { { new { Name = "Test1" } }, { new { Name = "Test2" } } };
            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"Dim arrayOfAnonymousType = {
    {
        New With {
            .Name = ""Test1""
        }
    },
    {
        New With {
            .Name = ""Test2""
        }
    }
}
", result);
        }

        [Fact]
        public void SerializeArrayOfArraysAnonymousVb()
        {
            var array = new[] { new[] { new { Name = "Clark" } } };

            var serializer = new VisualBasicSerializer();

            var result = serializer.Serialize(array, null);

            Assert.Equal(
@"Dim arrayOfArrayOfAnonymousType = {
    {
        New With {
            .Name = ""Clark""
        }
    }
}
", result);
        }
    }
}