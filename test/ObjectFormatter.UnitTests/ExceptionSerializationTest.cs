﻿using ObjectFormatter.Implementation;

namespace ObjectFormatter.UnitTests
{
    public class ExceptionSerializationTest
    {
        [Fact]
        public void SerializeExceptionCSharp()
        {
            try
            {
                var indexOutOfRange = new[] { "test" }[1];
            }
            catch (Exception e)
            {
                var serializer = new VisualBasicSerializer();

                var result = serializer.Serialize(e, null);
            }

        }
    }
}