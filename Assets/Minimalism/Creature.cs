using UnityEngine;
using System.Collections;

public class Creature {
	
	public enum TileCreatureType { HORSE, HAWK, CAMEL, FISH, GOAT }
	
	public TileCreatureType CreatureType { get; private set; }
	public Tile.TileGroundType HomeGround { get; private set; }
	
	public FSprite TileSprite { get; private set; }
	
	// Use this for initialization
	public Creature(TileCreatureType creatureType, Tile.TileGroundType homeGround) {
		this.CreatureType = creatureType;
		this.HomeGround = homeGround;
		TileSprite = GetSpriteRepresentation();
	}
	
	public FSprite GetSpriteRepresentation() {
		FSprite sprite = null;
		
		switch (CreatureType) {
		case TileCreatureType.HORSE:
			sprite = new FSprite("horse-white");
			break;
		case TileCreatureType.HAWK:
			sprite = new FSprite("hawk-white");
			break;
		case TileCreatureType.CAMEL:
			sprite = new FSprite("camel-white");
			break;
		case TileCreatureType.FISH:
			sprite = new FSprite("fish-white");
			break;
		case TileCreatureType.GOAT:
			sprite = new FSprite("goat-white");
			break;
		default:
			Debug.LogError("Unsupported tile type.");
			break;
		}
		
		return sprite;
	}
	
	public static Creature GetRandomCreatureForTile(Tile t) {
		Creature c = null;
		
		switch (t.GroundType) {
		case Tile.TileGroundType.GRASS:
			c = new Creature(TileCreatureType.HORSE, t.GroundType);
			break;
		case Tile.TileGroundType.FOREST:
			c = new Creature(TileCreatureType.HAWK, t.GroundType);
			break;
		case Tile.TileGroundType.DESERT:
			c = new Creature(TileCreatureType.CAMEL, t.GroundType);
			break;
		case Tile.TileGroundType.WATER:
			c = new Creature(TileCreatureType.FISH, t.GroundType);
			break;
		case Tile.TileGroundType.MOUNTAINS:
			c = new Creature(TileCreatureType.GOAT, t.GroundType);
			break;
		default:
			break;
		}
		
		return c;
	}
}
