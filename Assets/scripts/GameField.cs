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
using Assets.scripts;
using Random = System.Random;

namespace Assets.scripts
{
	public static class GameField
	{
		public static SpaceObject[,] Map;
		public static Coordinate? ClickedObject;
		public static float MoveSpeed = 10.0f;



		public static void Swap(Coordinate p1, Coordinate p2)
		{
			Map[p1.X, p1.Y].Move(Map[p2.X, p2.Y].transform.position);
			Map[p2.X, p2.Y].Move(Map[p1.X, p1.Y].transform.position);
			Map.SwapArrayElements(p1,p2);
			ClickedObject = null;
			Map[p1.X, p1.Y].GridPosition = p1;
			Map[p2.X, p2.Y].GridPosition = p2;
		}

		public static void UpdateField()
		{

			for (int i = 0; i < Map.GetLength(0); i++)
			{
				for (int j = 0; j < Map.GetLength(1); j++)
				{
					if (Map[i, j] == null || !Map[i, j].IsAsteroid())
						continue;
					CheckColumn(Map[i, j], i, j);
					if (Map[i,j] == null || !Map[i,j].IsAsteroid())
						continue;
					CheckRow(Map[i, j], i, j); //checking row here
				}
			}
			DropAsteroids();
			//for (int i = 0; i< 8; i++)
			//	for (int j = 0; j < 8; j++)
			//	{
			//		if (Map[i,j] == null)
			//			Debug.Log(i+" "+j);
			//	}
			//yield return null;
		}
		private static void CheckRow(SpaceObject cell, int i, int j)
		{

			var asteroidsColumnList = new List<Coordinate>();
			asteroidsColumnList.Add(cell.GridPosition);
			while (i < Map.GetLength(0) - 1 && Map[i + 1, j] != null 
				&& Map[i + 1, j].TypeOfObject == cell.TypeOfObject)
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

		private static void DropAsteroids()
		{
			var wasDropped = false;
			Random r = new Random();
			for (int j = Map.GetLength(1) - 1; j >= 0; j--)
			{
				for (int i = 0; i < Map.GetLength(0); i++)
				{
					if (Map[i, j] != null)
						continue;
					var depth = j;
					while ( depth > 0 && Map[i, depth] == null)
					{
						depth--;
					}
					if (depth == 0 && Map[i, depth] == null)
					{
						wasDropped = true;
						Game.SpaceObjectCreate(i, 0, AsteroidsList.ElementAt(r.Next(5)));
					}
					if (depth >= 0  && depth != j && Map[i, depth] != null)
					{
						Map[i, j] = Map[i, depth];
						Map[i, depth] = null;
						Map[i,j].GridPosition = new Coordinate(i, j);
						Map[i,j].Destination = GetVectorFromCoord(i,j);
					}
				}	 
			}
		}

		public static Vector3 GetVectorFromCoord(int i, int j)
		{
			return new Vector3(i - 4,
							4 - j, 0);
		}

		public static List<char> AsteroidsList = new List<char>()
		{
			'P',
			'R',
			'Y',
			'B',
			'G',
		}; 

		public static bool IsAnyMoving()
		{
			for (int i = 0; i < Game.MAP_SIZE; i++)
				for (int j = 0; j < Game.MAP_SIZE; j++)
					if (Map[i,j] != null && 
						(Map[i, j].State == SpaceObjectState.Dropping 
						|| Map[i, j].State == SpaceObjectState.Moving))
							return true;
			return false;
		}
		private static void SwapArrayElements<T>(this T[,] inputArray, Coordinate index1, Coordinate index2)
		{
			T temp = inputArray[index1.X, index1.Y];
			inputArray[index1.X, index1.Y] = inputArray[index2.X, index2.Y];
			inputArray[index2.X, index2.Y] = temp;
		}
	}
}
