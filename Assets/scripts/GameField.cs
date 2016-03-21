using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.scripts
{
	public static class GameField
	{
		public static SpaceObject[,] Map;
		public static Coordinate? ClickedObject;
		public static float MoveSpeed = 1.0f;
		public static void Swap(Coordinate p1, Coordinate p2)
		{
			Debug.Log("swapping "  + p1.X + " " + p1.Y + " with " + p2.X + " " + p2.Y);
			var tempPos = Map[p1.X, p1.Y].transform.position;
			Map[p1.X, p1.Y].transform.position = Map[p2.X, p2.Y].transform.position;
			Map[p2.X, p2.Y].transform.position = tempPos;
			Map.SwapArrayElements(p1,p2);
			ClickedObject = null;
			Map[p1.X, p1.Y].GridPosition = p1;
			Map[p2.X, p2.Y].GridPosition = p2;


		}
		private static void SwapArrayElements<T>(this T[,] inputArray, Coordinate index1, Coordinate index2)
		{
			T temp = inputArray[index1.X, index1.Y];
			inputArray[index1.X, index1.Y] = inputArray[index2.X, index2.Y];
			inputArray[index2.X, index2.Y] = temp;
		}
	}
}
