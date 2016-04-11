using System;
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
		public static GameObject SpaceObjectPrefab;
		public const int MAP_SIZE = 8;
		private LevelInfo LevelInformation;
		public static int TurnsLeft;
		public static Game instance;
		private void Start()
		{
			SpaceObjectPrefab = Resources.Load("SpaceObjectPrefab", typeof(GameObject)) as GameObject;
			SpaceObject.BlueAsteroidSprite = Resources.Load("1met_blue", typeof(Sprite)) as Sprite;
			SpaceObject.GreenAsteroidSprite = Resources.Load("1met_green", typeof(Sprite)) as Sprite;
			SpaceObject.RedAsteroidSprite = Resources.Load("1met_red", typeof(Sprite)) as Sprite;
			SpaceObject.PurpleAsteroidSprite = Resources.Load("1met_purple", typeof(Sprite)) as Sprite;
			SpaceObject.YellowAsteroidSprite = Resources.Load("1met_yellow", typeof(Sprite)) as Sprite;
			SpaceObject.EmptyCellSprite = Resources.Load("EmptyCell", typeof(Sprite)) as Sprite;
			SpaceObject.UnstableBlueAsteroidSprite = Resources.Load("2Nmet_blue", typeof(Sprite)) as Sprite;
			SpaceObject.UnstableGreenAsteroidSprite = Resources.Load("2Nmet_green", typeof(Sprite)) as Sprite;
			SpaceObject.UnstableRedAsteroidSprite = Resources.Load("2Nmet_red", typeof(Sprite)) as Sprite;
			SpaceObject.UnstablePurpleAsteroidSprite = Resources.Load("2Nmet_purple", typeof(Sprite)) as Sprite;
			SpaceObject.UnstableYellowAsteroidSprite = Resources.Load("2Nmet_yellow", typeof(Sprite)) as Sprite;
			SpaceObject.BlackHoleSprite = Resources.Load("3Black_hole_01", typeof(Sprite)) as Sprite;
			SpaceObject.IceSprite = Resources.Load("3Space_ice", typeof(Sprite)) as Sprite;

			SpaceObject.SpaceObjectTypesToSprites = new Dictionary<SpaceObjectType, Sprite>
			{
				{SpaceObjectType.GreenAsteroid, SpaceObject.GreenAsteroidSprite},
				{SpaceObjectType.RedAsteroid, SpaceObject.RedAsteroidSprite},
				{SpaceObjectType.BlueAsteroid, SpaceObject.BlueAsteroidSprite},
				{SpaceObjectType.PurpleAsteroid, SpaceObject.PurpleAsteroidSprite},
				{SpaceObjectType.YellowAsteroid, SpaceObject.YellowAsteroidSprite},
				{SpaceObjectType.EmptyCell, SpaceObject.EmptyCellSprite},
				{SpaceObjectType.BlackHole, SpaceObject.BlackHoleSprite},
				{SpaceObjectType.Ice, SpaceObject.IceSprite}
			};
			SpaceObject.StableToUnstableSprites = new Dictionary<SpaceObjectType, Sprite>
			{
				{SpaceObjectType.GreenAsteroid, SpaceObject.UnstableGreenAsteroidSprite},
				{SpaceObjectType.RedAsteroid, SpaceObject.UnstableRedAsteroidSprite},
				{SpaceObjectType.BlueAsteroid, SpaceObject.UnstableBlueAsteroidSprite},
				{SpaceObjectType.PurpleAsteroid, SpaceObject.UnstablePurpleAsteroidSprite},
				{SpaceObjectType.YellowAsteroid, SpaceObject.UnstableYellowAsteroidSprite},

			};
			instance = this;
			TurnsLeft = 10;
			//LevelInformation = JsonConvert.DeserializeObject<LevelInfo>(File.ReadAllText("Assets/levels/1.json"));
			LevelInformation =
			new LevelInfo { Map = "GBPYYRYY GHGRRYHB RYGYYBIB GRREEEPR RYGYPGPB GPGPYGRP RYRBGBRB GYGYYGPG" };
			//new LevelInfo { Map = "GBPY EYGR RYYE GRYR RYGY GPGP RYRB GYGY" };
			GenerateMap();

		}

		// Update is called once per frame
		public void Update()
		{
			//if (TurnsLeft == 0)
				//Debug.Log("Game Is Finished");
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
