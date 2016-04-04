﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Assets.scripts;
//using Newtonsoft.Json;
namespace Assets.scripts
{
	public class Game : MonoBehaviour
	{

		// Use this for initialization
		public static GameObject SpaceObjectPrefab = Resources.Load("SpaceObjectPrefab", typeof(GameObject)) as GameObject;
		public const int MAP_SIZE = 4;
		private LevelInfo LevelInformation;
		public static int TurnsLeft;
		private void Start()
		{
			TurnsLeft = 10;
			//LevelInformation = JsonConvert.DeserializeObject<LevelInfo>(File.ReadAllText("Assets/levels/1.json"));
			LevelInformation =
			//new LevelInfo { Map = "GBPYYRPR GYGRRGPB RYGYYBRB GRYRGRPR RYGYPGPB GPGPYGRP RYRBGBRB GYGYYGPG" };
			new LevelInfo { Map = "GBPY EYGR RYYE GRYR RYGY GPGP RYRB GYGY" };
			GenerateMap();

		}

		// Update is called once per frame
		private void Update()
		{
			if (TurnsLeft == 0)
				Debug.Log("Game Is Finished");
			GameField.CheckUpperBorder();
			if (!GameField.IsAnyMoving())
			{
				if (!GameField.IsAnyCorrectMove())
					GameField.Shuffle();
				GameField.UpdateField();
			}
		}

		private void GenerateMap()
		{
			GameField.Map = new SpaceObject[MAP_SIZE, MAP_SIZE];
			for (var i = 0; i < MAP_SIZE; i++)
			{
				for (var j = 0; j < MAP_SIZE; j++)
				{
					SpaceObjectCreate(i, j, LevelInformation.Map.ElementAt(j*(MAP_SIZE + 1) + i));
				}
			}
		}

		public static void SpaceObjectCreate(int x, int y, char type, float delay = 0, bool isUnstable = false)
		{
			SpaceObject spaceObject = ((GameObject)Instantiate(
						SpaceObjectPrefab, GameField.GetVectorFromCoord(x,y),
						Quaternion.Euler(new Vector3())))
						.GetComponent<SpaceObject>();
			spaceObject.Initialise(x, y, type, delay, isUnstable); //?
			GameField.Map[x, y] = spaceObject;
		}

	}
}
