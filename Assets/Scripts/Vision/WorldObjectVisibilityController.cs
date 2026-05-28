using UnityEngine;

/*
 * WorldObjectVisibilityController
 * -------------------------------
 * Controls whether runtime world objects are visually shown or hidden based on
 * the player's field of view.
 *
 * The fog tilemaps hide the map visually, but actors/items/features should also
 * have proper visibility rules so enemies and loot are not visible through fog.
 *
 * Current visibility rules:
 * - actors are visible only if their grid cell is currently visible
 * - items are visible only if their grid cell is currently visible
 * - features can be visible when explored, so doors/stairs can be remembered
 *
 * Main responsibilities:
 * - receive the active PlayerFieldOfView
 * - listen for FOV refresh events
 * - update SpriteRenderer visibility under actor/item/feature parents
 * - avoid global object searches by using known scene parent transforms
 *
 * Important:
 * This script should live on an always-active scene object.
 * It does not disable GameObjects. It only enables/disables SpriteRenderers.
 *
 * Later this can expand into:
 * - hiding nameplates/health bars
 * - dimming explored features
 * - separate stealth visibility
 * - light-source visibility
 * - invisible enemies
 */

public class WorldObjectVisibilityController : MonoBehaviour
{
    [Header("Runtime Parents")]
    [SerializeField] private Transform actorParent;
    [SerializeField] private Transform itemParent;
    [SerializeField] private Transform featureParent;

    [Header("Feature Visibility")]
    [SerializeField] private bool showExploredFeatures = true;

    private PlayerFieldOfView playerFieldOfView;

    public void SetTarget(PlayerFieldOfView newPlayerFieldOfView)
    {
        if (playerFieldOfView != null)
        {
            playerFieldOfView.VisibilityRefreshed -= RefreshVisibility;
        }

        playerFieldOfView = newPlayerFieldOfView;

        if (playerFieldOfView != null)
        {
            playerFieldOfView.VisibilityRefreshed += RefreshVisibility;
        }

        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (playerFieldOfView == null)
        {
            return;
        }

        UpdateActorVisibility();
        UpdateItemVisibility();
        UpdateFeatureVisibility();
    }

    private void UpdateActorVisibility()
    {
        if (actorParent == null)
        {
            return;
        }

        for (int i = 0; i < actorParent.childCount; i++)
        {
            Transform child = actorParent.GetChild(i);

            if (child == null)
            {
                continue;
            }

            ActorGridEntity actor = child.GetComponent<ActorGridEntity>();

            if (actor == null)
            {
                continue;
            }

            bool isVisible = playerFieldOfView.IsCellVisible(actor.GridPosition);
            SetSpriteRenderersVisible(child, isVisible);
        }
    }

    private void UpdateItemVisibility()
    {
        if (itemParent == null)
        {
            return;
        }

        for (int i = 0; i < itemParent.childCount; i++)
        {
            Transform child = itemParent.GetChild(i);

            if (child == null)
            {
                continue;
            }

            ItemGridEntity item = child.GetComponent<ItemGridEntity>();

            if (item == null)
            {
                continue;
            }

            bool isVisible = playerFieldOfView.IsCellVisible(item.GridPosition);
            SetSpriteRenderersVisible(child, isVisible);
        }
    }

    private void UpdateFeatureVisibility()
    {
        if (featureParent == null)
        {
            return;
        }

        for (int i = 0; i < featureParent.childCount; i++)
        {
            Transform child = featureParent.GetChild(i);

            if (child == null)
            {
                continue;
            }

            MapFeatureEntity feature = child.GetComponent<MapFeatureEntity>();

            if (feature == null)
            {
                continue;
            }

            bool isVisible;

            if (showExploredFeatures)
            {
                isVisible = playerFieldOfView.IsCellExplored(feature.GridPosition);
            }
            else
            {
                isVisible = playerFieldOfView.IsCellVisible(feature.GridPosition);
            }

            SetSpriteRenderersVisible(child, isVisible);
        }
    }

    private void SetSpriteRenderersVisible(Transform root, bool isVisible)
    {
        SpriteRenderer[] spriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
            {
                continue;
            }

            spriteRenderers[i].enabled = isVisible;
        }
    }
}