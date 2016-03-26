using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
	public static class ExtensionMethods
	{
		public static void SwapArrayElements<T>(this T[,] inputArray, Coordinate index1, Coordinate index2)
		{
			T temp = inputArray[index1.X, index1.Y];
			inputArray[index1.X, index1.Y] = inputArray[index2.X, index2.Y];
			inputArray[index2.X, index2.Y] = temp;
		}


		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.Shuffle(new Random());
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (rng == null) throw new ArgumentNullException("rng");

			return source.ShuffleIterator(rng);
		}

		private static IEnumerable<T> ShuffleIterator<T>(
			this IEnumerable<T> source, Random rng)
		{
			var buffer = source.ToList();
			for (int i = 0; i < buffer.Count; i++)
			{
				int j = rng.Next(i, buffer.Count);
				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}
	}
}
