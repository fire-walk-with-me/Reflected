using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    //List of audioclips for enemy attacks
    [Header("AudioClips for respective attacks")]
    [SerializeField] List<AudioClip> rangedClips;
    [SerializeField] List<AudioClip> meleeClips;
    [SerializeField] List<AudioClip> explosionChargeClips;
    [SerializeField] List<AudioClip> explosionClips;

    [Header("Enemy Specific Properties (If Relevant)")]
    [SerializeField] protected float baseProjectileSpeed;
    [SerializeField] protected Vector3 baseAoeSize;
    //[SerializeField] protected float fleeRange; //Not gonna be adabted
    //[SerializeField] protected float chaseRange; //Not gonna be adabted
    
    //Are these still used? The manager should be taking care of these right? -Kevin
    [SerializeField] protected Vector3 combatTextOffset;
    [SerializeField] protected Canvas combatTextCanvas;

    [SerializeField] protected WeightedRandomList<GameObject> LootDropList;
    //[SerializeField] protected List<AudioClip> hitSounds;
    protected bool invurnable;
    GameObject parent;
    protected Player player;
    EnemyStatSystem statSystem;
    //protected float aggroRange;

    bool doOnce;

    [SerializeField] MeleeAttackState meleeAttackState;
    [SerializeField] RangedAttackState rangedAttackState;
    [SerializeField] AoeAttackState aoeAttackState;
    [SerializeField] ExplosionAttackState explosionAttackState;

    protected override void Awake()
    {
        statSystem = FindObjectOfType<EnemyStatSystem>();
        
        currentHealth = maxHealth + statSystem.GetMaxHealthIncrease();
        base.Awake();
        player = FindObjectOfType<Player>();
        parent = gameObject.transform.parent.gameObject;

        //Assign correct components depending on enemy type.
        if (parent.tag == "Melee")
        {
            meleeAttackState = GetComponentInParent<MeleeAttackState>();
        }
        else if (parent.tag == "Ranged")
        {
            rangedAttackState = GetComponentInParent<RangedAttackState>();
        }
        else if (parent.tag == "AOE")
        {
            aoeAttackState = GetComponentInParent<AoeAttackState>();
        }
        else if (parent.tag == "Explosion")
        {
            explosionAttackState = GetComponentInParent<ExplosionAttackState>();
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    private void OnEnable()
    {
        EnemyStatSystem.OnStatsChanged += UpdateEnemyHealth;
    }

    private void OnDisable()
    {
        EnemyStatSystem.OnStatsChanged -= UpdateEnemyHealth;
    }

    public override void TakeDamage(float damage)
    {
        if (invurnable)
            return;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        damage = Mathf.FloorToInt(damage * statSystem.GetDamageReduction());
        //Call base take damage function
        base.TakeDamage(damage);
    }

    public void UpdateEnemyHealth()
    {
        if(statSystem.GetMaxHealthIncrease() > 0)
        {
            Heal(statSystem.GetMaxHealthIncrease());
        }
        else if(currentHealth > GetMaxHealth())
        {
            currentHealth = GetMaxHealth();
        }
    }

    public override void Heal(float amount)
    {
        PopUpTextManager.NewHeal(transform.position + Vector3.up * 1.5f, amount);
        currentHealth += Mathf.Clamp(amount, 0, maxHealth + statSystem.GetMaxHealthIncrease() - currentHealth);
        HealthChanged.Invoke();
    }

    protected override void Die()
    {
        //Let the director know an enemy in the room has died.
        AiDirector aiDirector = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AiDirector>();
        if (!doOnce)
        {
            aiDirector.killEnemyInRoom();
            doOnce = true;
        }

        //Drop loot if not a boss
        if (!GetComponent<Boss>())
            LootDrop(transform);

        //Call base die function
        base.Die();
    }

    public void AdaptiveDifficulty(float extraDifficultyPercentage) //called when instantiated (from the EnemySpanwer-script)
    {
        maxHealth += maxHealth * extraDifficultyPercentage;
        currentHealth = maxHealth + statSystem.GetMaxHealthIncrease();
        attackSpeed += attackSpeed * (extraDifficultyPercentage * 0.3f);
        movementSpeed += movementSpeed * (extraDifficultyPercentage * 0.2f);
        damage += damage * (extraDifficultyPercentage * 0.8f);
    }

    public virtual void LootDrop(Transform lootDropPosition)
    {
        LootDropList = GameObject.Find("LootPoolManager").GetComponent<LootPoolManager>().GetCollectablePool();

        Vector3 spawnPosition = lootDropPosition.position + new Vector3(0, 1, 0);
        Instantiate(LootDropList.GetRandom(), spawnPosition, Quaternion.Euler(0, 0, 0));
    }

    public virtual void ToggleInvurnable()
    {
        invurnable = !invurnable;
    }

    public override float GetHealthPercentage()
    {
        return currentHealth / (maxHealth + statSystem.GetMaxHealthIncrease());
    }

    public override float GetMaxHealth()
    {
        return maxHealth + statSystem.GetMaxHealthIncrease();
    }

    public void DoAttack() //Called from the animation, that then calls the correct attack depending on enemy type.
    {
        if (parent.tag == "Melee")
        {
            meleeAttackState.DoAttack();
        }
        else if (parent.tag == "Ranged")
        {
            rangedAttackState.DoAttack();
        }
        else if (parent.tag == "AOE")
        {
            aoeAttackState.DoAttack();
        }
        else if (parent.tag == "Explosion")
        {
            explosionAttackState.DoAttack(this);
        }
    }

    public void PlayAttackSFX() //Called from the animation
    {
        GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);

        if (parent.tag == "Melee")
        {
            GetComponent<AudioSource>().PlayOneShot(meleeClips.GetRandom());
        }
        else if (parent.tag == "Ranged")
        {
            GetComponent<AudioSource>().PlayOneShot(rangedClips.GetRandom());
        }
        else if (parent.tag == "AOE")
        {
            GetComponent<AudioSource>().PlayOneShot(rangedClips.GetRandom());
        }
        else if (parent.tag == "Explosion")
        {
            GetComponent<AudioSource>().PlayOneShot(explosionChargeClips.GetRandom());
        }
    }

    public void PlayExplosionSFX() //Called from the animation
    {
        GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
        GetComponent<AudioSource>().PlayOneShot(explosionClips.GetRandom());
    }

    public float GetProjectileSpeed()
    {
        return baseProjectileSpeed;
    }
    public Vector3 GetAoeSize()
    {
        return baseAoeSize;
    }
}
