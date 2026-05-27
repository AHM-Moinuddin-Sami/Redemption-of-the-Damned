using UnityEngine;

/*
 * StairsDownFeature
 * -----------------
 * Represents stairs that move the player to the next generated dungeon floor.
 *
 * This script now sends the descent message to the in-game message log.
 */

[RequireComponent(typeof(MapFeatureEntity))]
public class StairsDownFeature : MonoBehaviour
{
    private GameBootstrap gameBootstrap;
    private MapFeatureEntity mapFeatureEntity;

    private void Awake()
    {
        mapFeatureEntity = GetComponent<MapFeatureEntity>();
    }

    public bool Initialize(
        MapData mapData,
        MapRenderer mapRenderer,
        Vector2Int startPosition,
        GameBootstrap newGameBootstrap)
    {
        gameBootstrap = newGameBootstrap;

        return mapFeatureEntity.Initialize(mapData, mapRenderer, startPosition);
    }

    public void Use()
    {
        if (gameBootstrap == null)
        {
            Debug.LogWarning("StairsDownFeature cannot be used because GameBootstrap is missing.");
            return;
        }

        GameMessageLog.Write("You descend to the next floor.");

        gameBootstrap.GoToNextFloor();
    }
}