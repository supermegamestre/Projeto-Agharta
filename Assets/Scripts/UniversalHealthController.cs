using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UniversalHealthController : MonoBehaviour
{
    [HideInInspector]
    public int currentLife, currentShield;
    private bool invincible = false, hasShield = true;
    [SerializeField]
    private bool npc = false, hasInvincibilityFrames = false;
    [SerializeField]
    private Transform respawn;
    [SerializeField, Range(1, 10)]
    private int life = 1;
    [SerializeField, Range(0, 10)]
    private int shield = 1;
    [SerializeField, Range(0.1f, 10f)]
    private float invincibilityDuration = 1f;
    [SerializeField, Range(0f, 10f)]
    private float respawnTimer = 1f;
    [SerializeField]
    private SpriteRenderer character;
    [SerializeField]
    private GameObject characterObject;

    private void Start()
    {
        currentLife = life;
        currentLife = Mathf.Clamp(currentLife, 0, life);
        if(shield > 0)
            currentShield = shield;
        else
            hasShield = false;
        currentShield = Mathf.Clamp(currentShield, 0, shield);
    }

    public void gotHit(int damage)
    {
        if (!invincible)
        {
            if(hasShield && currentShield > 0)
                currentShield -= damage;
            else if(currentLife > 0)
                currentLife -= damage;
            if (currentLife == 0 && !npc)
                respawn.GetComponent<UniversalRespawn>().respawn(npc, this.gameObject, respawnTimer);
            else if (currentLife == 0)
            {
                if (characterObject.TryGetComponent<EnemyAI>(out EnemyAI enemyAI))
                    enemyAI.endOfLife();
                else if (characterObject.TryGetComponent<DestructibleObject>(out DestructibleObject destructibleObject))
                    destructibleObject.endOfLife();
                else
                    characterObject.SetActive(false);
                    
            }
            else if (hasInvincibilityFrames)
                StartCoroutine(invincibilityFrames());
        }
    }

    private IEnumerator invincibilityFrames()
    {
        invincible = true;
        StartCoroutine(invincibilityFramesEffect());
        yield return new WaitForSeconds(invincibilityDuration);
        invincible = false;
        yield break;
    }

    private IEnumerator invincibilityFramesEffect()
    {
        while (invincible)
        {
            character.enabled = false;
            yield return new WaitForSecondsRealtime(0.1f);
            character.enabled = true;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    public void respawned()
    {
        currentLife = life;
        if (shield > 0)
            currentShield = shield;
        else
            hasShield = false;
    }

    public void heal(int healing, bool shieldOverlap)
    {
        int result = currentLife + healing;
        if(result > life && shieldOverlap)
            currentShield += (result - life);
        currentLife += healing;
    }

    public void shieldRegen(int regen)
    {
        currentShield += regen;
    }
}
