using System;
using System.Collections.Generic;

namespace YellowFlavor.Serialization.Embedded.CodeDom
{

#if !NET6_0_OR_GREATER
    internal static class EnumerableExtensions
    {
        /// <summary>Split the elements of a sequence into chunks of size at most <paramref name="size" />.</summary>
        /// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements to chunk.</param>
        /// <param name="size">The maximum size of each chunk.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="source" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="size" /> is below 1.</exception>
        /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the elements the input sequence split into chunks of size <paramref name="size" />.</returns>
        public static IEnumerable<TSource[]> Chunk<TSource>(
          this IEnumerable<TSource> source,
          int size)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));

            return ChunkIterator(source, size);
        }


#nullable disable
        private static IEnumerable<TSource[]> ChunkIterator<TSource>(
          IEnumerable<TSource> source,
          int size)
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                TSource[] array = new TSource[size];
                array[0] = enumerator.Current;
                int newSize;
                for (newSize = 1; newSize < array.Length && enumerator.MoveNext(); ++newSize)
                    array[newSize] = enumerator.Current;
                if (newSize == array.Length)
                {
                    yield return array;
                }
                else
                {
                    Array.Resize(ref array, newSize);
                    yield return array;
                    break;
                }
            }
        }
    }
#endif
}
