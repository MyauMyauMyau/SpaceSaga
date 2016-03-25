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
	}
}
