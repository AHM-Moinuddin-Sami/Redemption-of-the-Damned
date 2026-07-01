using UnityEngine;

/*
 * EnemyDefinitionApplier
 * ----------------------
 * Applies an EnemyDefinition to a spawned enemy prefab.
 *
 * This lets one generic enemy prefab become different enemy types at runtime.
 *
 * Current responsibilities:
 * - set enemy display name
 * - set enemy sprite
 * - set enemy base stats
 * - refill enemy health after stats are applied
 * - set XP reward
 * - set basic AI settings
 *
 * Required components on the enemy prefab:
 * - ActorGridEntity
 * - ActorStats
 * - ActorHealth
 * - SimpleEnemyAI
 * - ExperienceReward
 * - SpriteRenderer
 *
 * Important:
 * This script should be placed on the generic enemy prefab.
 */

[RequireComponent(typeof(ActorGridEntity))]
[RequireComponent(typeof(ActorStats))]
[RequireComponent(typeof(ActorHealth))]
[RequireComponent(typeof(SimpleEnemyAI))]
[RequireComponent(typeof(ExperienceReward))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(EnemyDeathLoot))]
public class EnemyDefinitionApplier : MonoBehaviour
{
    private ActorGridEntity actorGridEntity;
    private ActorStats actorStats;
    private ActorHealth actorHealth;
    private SimpleEnemyAI simpleEnemyAI;
    private ExperienceReward experienceReward;
    private SpriteRenderer spriteRenderer;
    private EnemyDeathLoot enemyDeathLoot;

    private void Awake()
    {
        actorGridEntity = GetComponent<ActorGridEntity>();
        actorStats = GetComponent<ActorStats>();
        actorHealth = GetComponent<ActorHealth>();
        simpleEnemyAI = GetComponent<SimpleEnemyAI>();
        experienceReward = GetComponent<ExperienceReward>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyDeathLoot = GetComponent<EnemyDeathLoot>();
    }

    public void ApplyDefinition(EnemyDefinition enemyDefinition)
    {
        if (enemyDefinition == null)
        {
            return;
        }

        actorGridEntity.SetDisplayName(enemyDefinition.DisplayName);

        if (enemyDefinition.Sprite != null)
        {
            spriteRenderer.sprite = enemyDefinition.Sprite;
        }

        actorStats.SetBaseStats(
            enemyDefinition.MaxHealth,
            enemyDefinition.AttackDamage,
            enemyDefinition.Armor
        );

        actorHealth.SetToFullHealth();

        experienceReward.SetExperienceAmount(enemyDefinition.ExperienceReward);

        if (enemyDeathLoot != null)
        {
            enemyDeathLoot.ApplyDefinition(enemyDefinition);
        }

        simpleEnemyAI.ApplyEnemyDefinition(enemyDefinition);
    }
}