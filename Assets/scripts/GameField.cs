﻿using System;
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
	public class GameField
	{
		public static SpaceObject[,] Map;
		public static Coordinate? ClickedObject;
		public static float MoveSpeed = 10.0f;
		public static bool MoveIsFinished = true;
		public static Random Rnd = new Random();
		public static void CheckUpperBorder()
		{
			var delay = 0f;
			var step = 3f / SpaceObject.BaseDropSpeed;
			for (int i = 0; i < Map.GetLength(0); i++)
			{
				if (Map[i, 0] == null)
				{
					Game.SpaceObjectCreate(i, 0, AsteroidsList.ElementAt(Rnd.Next(5)), delay);
				}
			}
		}
		public static void Shuffle()
		{
			var rnd = new Random();
			var asteroidsCoordinates = new List<Coordinate>();
			for (int i = 0; i < Map.GetLength(0); i++)
			{
				for (int j = 0; j < Map.GetLength(1); j++)
					if (Map[i,j] != null && Map[i, j].IsAsteroid())
						asteroidsCoordinates.Add(new Coordinate(i, j));
			}
			while (asteroidsCoordinates.Count != 0)
			{
				MoveIsFinished = false;
				var target = rnd.Next(asteroidsCoordinates.Count);
				Swap(asteroidsCoordinates.ElementAt(0), asteroidsCoordinates.ElementAt(target));
				asteroidsCoordinates.RemoveAt(0);
				if (target != 0)
					asteroidsCoordinates.RemoveAt(target -1);
			}
		}
		public static bool IsAnyCorrectMove()
		{
			for (int i = 0; i < Map.GetLength(0)-1; i++)
			{
				for (int j = 0; j < Map.GetLength(1); j++)
				{
					if (Map[i,j] == null || Map[i+1,j] == null)
						continue;
					if (!Map[i,j].IsAsteroid() || !Map[i+1,j].IsAsteroid())
						continue;
					var p1 = new Coordinate(i, j);
					var p2 = new Coordinate(i + 1, j);
					Map.SwapArrayElements(p1, p2);
					if (IsCorrectMove(new List<Coordinate>() {p1, p2}))
					{
						Map.SwapArrayElements(p1, p2);
						return true;
					}
					Map.SwapArrayElements(p1, p2);
				}
			}
			for (int i = 0; i < Map.GetLength(0); i++)
			{
				for (int j = 0; j < Map.GetLength(1) - 1; j++)
				{
					if (Map[i, j] == null || Map[i, j+1] == null)
						continue;
					if (!Map[i, j].IsAsteroid() || !Map[i, j + 1].IsAsteroid())
						continue;
					var p1 = new Coordinate(i, j);
					var p2 = new Coordinate(i, j + 1);
					Map.SwapArrayElements(p1, p2);
					if (IsCorrectMove(new List<Coordinate>() { p1, p2 }))
					{
						Map.SwapArrayElements(p1, p2);
						return true;
					}
					Map.SwapArrayElements(p1, p2);
				}
			}
			return false;
		}
		public static bool IsCorrectMove(List<Coordinate> coordinates)
		{
			//correct if any triples
			foreach (var coordinate in coordinates)
			{
				var bottomBoundX = Math.Max(0, coordinate.X - 2);
				var bottomBoundY = Math.Max(0, coordinate.Y - 2);
				var topBoundX = Math.Min(Map.GetLength(0) - 1, coordinate.X + 2);
				var topBoundY = Math.Min(Map.GetLength(1) - 1, coordinate.Y + 2);
				for (int i = bottomBoundX; i <= topBoundX - 2; i++)
				{
					if (Map[i, coordinate.Y] == null || Map[i+1, coordinate.Y] == null || Map[i+2, coordinate.Y] == null) continue;
					if (Map[i, coordinate.Y].TypeOfObject == Map[i + 1, coordinate.Y].TypeOfObject
					    && Map[i, coordinate.Y].TypeOfObject == Map[i + 2, coordinate.Y].TypeOfObject)
						return true;
				}
				for (int i = bottomBoundY; i <= topBoundY - 2; i++)
				{
					if (Map[coordinate.X, i] == null || Map[coordinate.X, i + 1] == null || Map[coordinate.X ,i + 2] == null) continue;
					if (Map[coordinate.X, i].TypeOfObject == Map[coordinate.X, i+1].TypeOfObject
						&& Map[coordinate.X, i].TypeOfObject == Map[coordinate.X, i+2].TypeOfObject)
						return true;
				}
			}
			return false;
		}

		public static void Swap(Coordinate p1, Coordinate p2)
		{
			Map[p1.X, p1.Y].Move(new Coordinate(p2.X, p2.Y));
			Map[p2.X, p2.Y].Move(new Coordinate(p1.X, p1.Y));
			Map.SwapArrayElements(p1,p2);
			ClickedObject = null;
			MoveIsFinished = !MoveIsFinished;
		}
		public static void Jump(Coordinate p1, Coordinate p2)
		{
			Map[p1.X, p1.Y].Move(p2);
			Map[p2.X, p2.Y].Move(p1);
			Map.SwapArrayElements(p1, p2);
			var temp = Map[p1.X, p1.Y].transform.position;
			Map[p1.X, p1.Y].transform.position = Map[p2.X, p2.Y].transform.position;
			Map[p2.X, p2.Y].transform.position = temp;
		}

		public static void Drop(Coordinate p1, Coordinate p2)
		{
			Map[p1.X, p1.Y].Move(p2);
			Map[p2.X, p2.Y] = Map[p1.X, p1.Y];
			Map[p1.X, p1.Y] = null;
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
		}



		private static void CheckRow(SpaceObject cell, int i, int j)
		{
			var unstableList = new List<Coordinate>();
			var asteroidsColumnList = new List<Coordinate>();
			asteroidsColumnList.Add(cell.GridPosition);
			var unstableIsAdded = false;
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
			if (asteroidsColumnList.Count >= 5)
			{
				var coord = asteroidsColumnList.ElementAt(asteroidsColumnList.Count/2);
				Map[coord.X, coord.Y].DestroyAsteroid();
				listToDestroy.Remove(coord);
				unstableList.Add(coord);
				unstableIsAdded = true;
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
				if (bufRowList.Count >= 5)
				{
					var coord = bufRowList.ElementAt(asteroidsColumnList.Count / 2);
					Map[coord.X, coord.Y].DestroyAsteroid();
					listToDestroy.Remove(coord);
					unstableList.Add(coord);
					unstableIsAdded = true;
				}
				if (bufRowList.Count >= 2)
				{
					//Debug.Log("yahoo");
					foreach (var coordinate in bufRowList)
					{
						listToDestroy.Add(coordinate);
					}
					if (!unstableIsAdded)
					{
						Map[asteroid.X, asteroid.Y].DestroyAsteroid();
						listToDestroy.Remove(asteroid);
						unstableList.Add(asteroid);
						unstableIsAdded = true;
					} 
				}
				bufRowList.Clear();
			}
			HandleUnstableAsteroids(listToDestroy);
			foreach (var coordinate in listToDestroy
				.Where(x => Map[x.X, x.Y] != null && Map[x.X, x.Y].IsAsteroid()))
			{
				Map[coordinate.X, coordinate.Y].DestroyAsteroid();
			}
			foreach (var coord in unstableList)
			{
				Game.SpaceObjectCreate(coord.X, coord.Y, SpaceObject.CharsToObjectTypes
						.First(x => x.Value == cell.TypeOfObject).Key, 0, true);
			}
		}
		private static void CheckColumn(SpaceObject cell, int i, int j)
		{
			var unstableList = new List<Coordinate>();
			var asteroidsColumnList = new List<Coordinate>();
			asteroidsColumnList.Add(cell.GridPosition);
			var unstableIsAdded = false;
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
			if (asteroidsColumnList.Count >= 5)
			{
				var coord = asteroidsColumnList.ElementAt(asteroidsColumnList.Count / 2);
				Map[coord.X, coord.Y].DestroyAsteroid();
				listToDestroy.Remove(coord);
				unstableList.Add(coord);
				unstableIsAdded = true;
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
				if (bufRowList.Count >= 5)
				{
					var coord = bufRowList.ElementAt(asteroidsColumnList.Count / 2);
					Map[coord.X, coord.Y].DestroyAsteroid();
					listToDestroy.Remove(coord);
					unstableList.Add(coord);
					unstableIsAdded = true;
				}
				if (bufRowList.Count >= 2)
				{
					foreach (var coordinate in bufRowList)
					{
						listToDestroy.Add(coordinate);
					}
					if (!unstableIsAdded)
					{
						Map[asteroid.X, asteroid.Y].DestroyAsteroid();
						listToDestroy.Remove(asteroid);
						unstableList.Add(asteroid);
						unstableIsAdded = true;
					}
				}
				bufRowList.Clear();
			}
			HandleUnstableAsteroids(listToDestroy);
			foreach (var coordinate in listToDestroy
				.Where(x=>Map[x.X, x.Y] != null && Map[x.X, x.Y].IsAsteroid()))
			{
				Map[coordinate.X, coordinate.Y].DestroyAsteroid();
			}
			foreach (var coord in unstableList)
			{
				Game.SpaceObjectCreate(coord.X, coord.Y, SpaceObject.CharsToObjectTypes
						.First(x => x.Value == cell.TypeOfObject).Key, 0, true);
			}
		}

		private static void HandleUnstableAsteroids(List<Coordinate> listToDestroy)
		{
			for (int i = 0; i < listToDestroy.Count; i++)
			{
				if (Map[listToDestroy[i].X, listToDestroy[i].Y] != null && Map[listToDestroy[i].X, listToDestroy[i].Y].IsUnstable)
				{
					Debug.Log("unstable");
					var bottomBoundX = Math.Max(0, listToDestroy[i].X - 2);
					var bottomBoundY = Math.Max(0, listToDestroy[i].Y - 2);
					var topBoundX = Math.Min(Map.GetLength(0) - 1, listToDestroy[i].X + 2);
					var topBoundY = Math.Min(Map.GetLength(1) - 1, listToDestroy[i].Y + 2);
					;
					for (int x = bottomBoundX; x <= topBoundX; x++)
					{
						var coord = new Coordinate(x, listToDestroy[i].Y);
						if (!listToDestroy.Contains(coord))
							listToDestroy.Add(coord);
					}
					for (int y = bottomBoundY; y <= topBoundY; y++)
					{
						var coord = new Coordinate(listToDestroy[i].X, y);
						if (!listToDestroy.Contains(coord))
							listToDestroy.Add(coord);
					}
				}
			}
		}

		private static void DropAsteroids()
		{
			var delay = 0f;
			var step = 3f / SpaceObject.BaseDropSpeed;
			Random r = new Random();
			for (int i = 0; i < Map.GetLength(0); i++)
			{
				delay = step/3;
				for (int j = Map.GetLength(1) - 1; j >= 0; j--)
				{
					if (Map[i, j] != null)
						continue;
					var depth = j;
					while ( depth > 0 && Map[i, depth] == null)
					{
						depth--;
					}
					if (Map[i, 0] == null)
					{
						Game.SpaceObjectCreate(i, 0, AsteroidsList.ElementAt(r.Next(5)), delay);
						delay += step;
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
			return new Vector3(i - Game.MAP_SIZE/2,
							Game.MAP_SIZE/2 - j, 0);
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
						|| Map[i, j].State == SpaceObjectState.Moving || Map[i, j].State == SpaceObjectState.Growing
						|| Map[i, j].State == SpaceObjectState.Decreasing))
							return true;
			return false;
		}
		
	}
}
