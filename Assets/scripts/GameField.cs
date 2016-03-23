using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
			//Debug.Log("swapping "  + p1.X + " " + p1.Y + " with " + p2.X + " " + p2.Y);
			Map[p1.X, p1.Y].Move(Map[p2.X,p2.Y].transform.position);
			Map[p2.X, p2.Y].Move(Map[p1.X, p1.Y].transform.position);
			Map.SwapArrayElements(p1,p2);
			ClickedObject = null;
			Map[p1.X, p1.Y].GridPosition = p1;
			Map[p2.X, p2.Y].GridPosition = p2;


		}

		public static IEnumerator UpdateField()
		{
			var isEnoughToDestroy = false;
			for (int i = 0; i < Map.GetLength(0); i++)
			{
				yield return null;
				for (int j = 0; j < Map.GetLength(1); j++)
				{
					var cell = Map[i, j];
					if (cell == null || !cell.IsAsteroid())
						continue;
					CheckColumn(cell, i, j);
					CheckRow(cell, i, j); //checking row here
				}
			}	
		}
		private static void CheckRow(SpaceObject cell, int i, int j)
		{

			var asteroidsColumnList = new List<Coordinate>();
			asteroidsColumnList.Add(cell.GridPosition);
			while (i < Map.GetLength(0) - 1 && Map[i + 1, j] != null && Map[i + 1, j].TypeOfObject == cell.TypeOfObject)
			{
				i++;
				asteroidsColumnList.Add(Map[i, j].GridPosition);
			}
			if (asteroidsColumnList.Count < 3)
				return;
			var listToDestroy = new List<Coordinate>();
			foreach (var coordinate in asteroidsColumnList)
			{
				listToDestroy.Add(coordinate);
			}
			var bufRowList = new List<Coordinate>();
			foreach (var asteroid in asteroidsColumnList)
			{
				j = asteroid.Y;
				i = asteroid.X;
				while (j < Map.GetLength(0) - 1
					   && Map[i, j + 1] != null && Map[i, j + 1].TypeOfObject == cell.TypeOfObject)
				{
					j++;
					bufRowList.Add(Map[i, j].GridPosition);
				}
				j = asteroid.Y;
				while (j > 0
					   && Map[i, j - 1] != null && Map[i, j - 1].TypeOfObject == cell.TypeOfObject)
				{
					j--;
					bufRowList.Add(Map[i, j].GridPosition);
				}
				Debug.Log(bufRowList.Count);
				if (bufRowList.Count >= 2)
				{
					Debug.Log("yahoo");
					foreach (var coordinate in bufRowList)
					{
						listToDestroy.Add(coordinate);
					}
				}
				bufRowList.Clear();
			}
			foreach (var coordinate in listToDestroy)
			{
				Map[coordinate.X, coordinate.Y].DestroyAsteroid();
				Map[coordinate.X, coordinate.Y] = null;
			}
		}
		private static void CheckColumn(SpaceObject cell, int i, int j)
		{
			var asteroidsColumnList = new List<Coordinate>();
			asteroidsColumnList.Add(cell.GridPosition);
			while (j < Map.GetLength(1) - 1 && Map[i, j + 1] != null && Map[i, j + 1].TypeOfObject == cell.TypeOfObject)
			{
				j++;
				asteroidsColumnList.Add(Map[i, j].GridPosition);
			}
			if (asteroidsColumnList.Count < 3)
				return;
			var listToDestroy = new List<Coordinate>();
			foreach (var coordinate in asteroidsColumnList)
			{
				listToDestroy.Add(coordinate);
			}
			var bufRowList = new List<Coordinate>();
			foreach (var asteroid in asteroidsColumnList)
			{
				j = asteroid.Y;
				i = asteroid.X;
				while (i < Map.GetLength(0) - 1
				       && Map[i + 1, j] != null && Map[i + 1, j].TypeOfObject == cell.TypeOfObject)
				{
					i++;
					bufRowList.Add(Map[i, j].GridPosition);
				}
				i = asteroid.X;
				while (i > 0
				       && Map[i - 1, j] != null && Map[i - 1, j].TypeOfObject == cell.TypeOfObject)
				{
					i--;
					bufRowList.Add(Map[i, j].GridPosition);
				}
				Debug.Log(bufRowList.Count);
				if (bufRowList.Count >= 2)
				{
					Debug.Log("yahoo");
					foreach (var coordinate in bufRowList)
					{
						listToDestroy.Add(coordinate);
					}
				}
				bufRowList.Clear();
			}
			foreach (var coordinate in listToDestroy)
			{
				Map[coordinate.X, coordinate.Y].DestroyAsteroid();
				Map[coordinate.X, coordinate.Y] = null;
			}
		}

		private static void SwapArrayElements<T>(this T[,] inputArray, Coordinate index1, Coordinate index2)
		{
			T temp = inputArray[index1.X, index1.Y];
			inputArray[index1.X, index1.Y] = inputArray[index2.X, index2.Y];
			inputArray[index2.X, index2.Y] = temp;
		}
	}
}
