using UnityEngine;

/*
 * DoorFeature
 * -----------
 * Represents an interactable door on the grid.
 *
 * This version:
 * - blocks movement and sight while closed
 * - allows movement and sight while open
 * - writes visibility-aware door messages
 * - emits door noise when opened or closed
 *
 * Door noise lets enemies investigate door usage.
 *
 * Important:
 * The sourceActor parameter lets enemies ignore their own door sounds.
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

    [Header("Sound Awareness")]
    [SerializeField] private int hearingRange = 8;

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

    public bool Toggle()
    {
        return Toggle(null);
    }

    public bool Toggle(ActorGridEntity sourceActor)
    {
        if (IsOpen)
        {
            return TryClose("You close the door.", "You hear a door close.", sourceActor);
        }

        return TryOpen("You open the door.", "You hear a door open.", sourceActor);
    }

    public bool TryOpen(string visibleMessage)
    {
        return TryOpen(visibleMessage, "You hear a door open.", null);
    }

    public bool TryOpen(string visibleMessage, string heardMessage)
    {
        return TryOpen(visibleMessage, heardMessage, null);
    }

    public bool TryOpen(string visibleMessage, string heardMessage, ActorGridEntity sourceActor)
    {
        if (IsOpen)
        {
            return false;
        }

        IsOpen = true;
        ApplyDoorState();

        WriteDoorMessage(visibleMessage, heardMessage);
        EmitDoorNoise(sourceActor);

        return true;
    }

    public bool TryClose(string visibleMessage)
    {
        return TryClose(visibleMessage, "You hear a door close.", null);
    }

    public bool TryClose(string visibleMessage, string heardMessage)
    {
        return TryClose(visibleMessage, heardMessage, null);
    }

    public bool TryClose(string visibleMessage, string heardMessage, ActorGridEntity sourceActor)
    {
        if (!IsOpen)
        {
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

        WriteDoorMessage(visibleMessage, heardMessage);
        EmitDoorNoise(sourceActor);

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

    private void WriteDoorMessage(string visibleMessage, string heardMessage)
    {
        if (mapFeatureEntity == null)
        {
            return;
        }

        Vector2Int position = mapFeatureEntity.GridPosition;

        if (PlayerAwarenessContext.CanSee(position))
        {
            if (!string.IsNullOrWhiteSpace(visibleMessage))
            {
                GameMessageLog.Write(visibleMessage);
            }

            return;
        }

        if (PlayerAwarenessContext.CanHear(position, hearingRange))
        {
            if (!string.IsNullOrWhiteSpace(heardMessage))
            {
                GameMessageLog.Write(heardMessage);
            }
        }
    }

    private void EmitDoorNoise(ActorGridEntity sourceActor)
    {
        if (mapFeatureEntity == null)
        {
            return;
        }

        GameNoiseSystem.EmitNoise(
            mapFeatureEntity.GridPosition,
            hearingRange,
            sourceActor,
            NoiseCategory.Door
        );
    }
}