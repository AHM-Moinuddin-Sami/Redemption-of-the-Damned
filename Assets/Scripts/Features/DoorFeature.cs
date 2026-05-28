using UnityEngine;

/*
 * DoorFeature
 * -----------
 * Represents an interactable door on the grid.
 *
 * Doors are map features. A closed door blocks movement and sight. An open door
 * allows movement and vision through.
 *
 * Current behavior:
 * - starts closed by default
 * - closed door blocks movement
 * - closed door blocks sight
 * - open door does not block movement or sight
 * - changing state updates the sprite
 * - closing is prevented if an actor is standing on the door tile
 */

[RequireComponent(typeof(MapFeatureEntity))]
[RequireComponent(typeof(SpriteRenderer))]
public class DoorFeature : MonoBehaviour
{
    [Header("Door State")]
    [SerializeField] private bool startsOpen = false;

    [Header("Sprites")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    public bool IsOpen { get; private set; }

    private MapData mapData;
    private MapFeatureEntity mapFeatureEntity;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        mapFeatureEntity = GetComponent<MapFeatureEntity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool Initialize(MapData newMapData, MapRenderer mapRenderer, Vector2Int startPosition)
    {
        mapData = newMapData;

        IsOpen = startsOpen;
        ApplyDoorState();

        bool placed = mapFeatureEntity.Initialize(mapData, mapRenderer, startPosition);

        if (!placed)
        {
            return false;
        }

        ApplyDoorState();

        return true;
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            TryClose();
            return;
        }

        Open();
    }

    public void Open()
    {
        if (IsOpen)
        {
            GameMessageLog.Write("The door is already open.");
            return;
        }

        IsOpen = true;
        ApplyDoorState();

        GameMessageLog.Write("You open the door.");
    }

    public bool TryClose()
    {
        if (!IsOpen)
        {
            GameMessageLog.Write("The door is already closed.");
            return false;
        }

        if (mapData != null)
        {
            ActorGridEntity actorOnDoor = mapData.GetActorAt(mapFeatureEntity.GridPosition);

            if (actorOnDoor != null)
            {
                GameMessageLog.Write("Something is blocking the door.");
                return false;
            }
        }

        IsOpen = false;
        ApplyDoorState();

        GameMessageLog.Write("You close the door.");

        return true;
    }

    private void ApplyDoorState()
    {
        if (mapFeatureEntity != null)
        {
            mapFeatureEntity.SetBlocksMovement(!IsOpen);
            mapFeatureEntity.SetBlocksSight(!IsOpen);
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (IsOpen)
        {
            if (openSprite != null)
            {
                spriteRenderer.sprite = openSprite;
            }

            return;
        }

        if (closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }
    }
}