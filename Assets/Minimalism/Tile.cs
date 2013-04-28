using UnityEngine;
using System.Collections;

public class Tile {
	
	public static readonly int TILE_SIZE_IN_PIXELS = 32;
	
	public enum TileGroundType { GRASS, FOREST, DESERT, WATER, MOUNTAINS, SWAMP }
	
	public TileGroundType GroundType { get; private set; }
	
	public FSprite TileSprite { get; private set; }
	
	// Use this for initialization
	public Tile(TileGroundType groundType) {
		this.GroundType = groundType;
		TileSprite = GetSpriteRepresentation();
	}
	
	public FSprite GetSpriteRepresentation() {
		FSprite sprite = null;
		
		switch (GroundType) {
		case TileGroundType.GRASS:
			sprite = new FSprite("tile-grass-light");
			break;
		case TileGroundType.FOREST:
			sprite = new FSprite("tile-grass-dark");
			break;
		case TileGroundType.DESERT:
			sprite = new FSprite("tile-sand");
			break;
		case TileGroundType.WATER:
			sprite = new FSprite("tile-water");
			break;
		case TileGroundType.MOUNTAINS:
			sprite = new FSprite("tile-mountains");
			break;
		case TileGroundType.SWAMP:
			sprite = new FSprite("tile-swamp");
			break;
		default:
			Debug.LogError("Unsupported tile type.");
			break;
		}
		
		return sprite;
	}
}
