using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStats : EntityStats
{
    CharacterData characterData;
    public CharacterData.Stats baseStats;
    [SerializeField] CharacterData.Stats actualStats;

    public CharacterData.Stats Stats
    {
        get { return actualStats; }
        set { actualStats = value; }
    }
    public CharacterData.Stats Actual
    {
        get { return actualStats; }
    }





    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return health; }
        //if we try and set the current health, the UI interface
        //on the pause screen will also be updated
        set
        {
            // check if value has changed
            if (health != value) 

            {
                health = value;  // update real time value of the stat
                updateHealthBar(); // update health bar UI
            }
        }
    }


    #endregion

    [Header("Visuals")]
    public ParticleSystem damageEffect;  // if damage is dealt
    public ParticleSystem blockedEffect; // if armor completely blocks damage

    // Experience and Level of the player
    [Header("Experience/Level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap;

    //I-Frames
    [Header("Iframes")]
    public float invincibilityDuration;
    float invincibilityTimer;
    bool isInvincible;

    [System.Serializable]
    public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int experienceCapIncrease;
    }

    public List<LevelRange> levelRanges;

    PlayerCollector collector;
    PlayerInventory inventory;


    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TextMeshProUGUI levelText;


    PlayerAnimator playerAnimator;
    void Awake()
    {
            
        characterData = UICharacterSelector.GetData();

        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();

        // Start from character's base stats, then apply permanent meta-upgrades
        CharacterData.Stats metaStats = MetaUpgradeManager.GetMetaStats();
        baseStats = actualStats = characterData.stats + metaStats;
        health = actualStats.maxHealth;
        collector.SetRadius(actualStats.magnet);

        playerAnimator = GetComponent<PlayerAnimator>();
        playerAnimator.SetAnimatorController(characterData.controller);
    }


    protected override void Start()
    {
        base.Start();
        //adds the global buff if there is any
        if(UILevelSelector.globalBuff && !UILevelSelector.globalBuffAffectsPlayer)
            ApplyBuff(UILevelSelector.globalBuff);
        //spawn the starting weapon
        inventory.Add(characterData.StartingWeapon);

        //initialize the experience cap as the first experience cap increase
        experienceCap = levelRanges[0].experienceCapIncrease;


        GameManager.instance.AssignChosenCharacterUI(characterData);


        updateHealthBar();
        UpdateLevelText();
        UpdateExpBar();
    }


    protected override void Update()
    {
        base.Update();
        if(invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        //if the invincibility timer reached 0 set invincibility flag to false
        else if (isInvincible)
        {
            isInvincible = false;
        }

        Recover();
    }

    public override void RecalculateStats()
    {
        actualStats = baseStats;
        foreach(PlayerInventory.Slot s in inventory.passiveSlots)
        {
            Passive p = s.item as Passive;
            if (p)
            {
                actualStats += p.GetBoosts();
            }
        }

        //create a variable to store all the cumulative multiplier values
        CharacterData.Stats multiplier = new CharacterData.Stats
        {
            maxHealth = 1f, recovery = 1f, armor = 1f, moveSpeed = 1f, might = 1f,
            area = 1f, speed = 1f, duration = 1f, ammount = 1, cooldown = 1f,
            luck = 1f, growth = 1f, greed = 1f, curse = 1f, magnet = 1f, revival = 1
        };

        foreach(Buff b in activeBuffs)
        {
            BuffData.Stats bd = b.GetData();
            switch (bd.modifierType)
            {
                case BuffData.ModifierType.additive:
                    actualStats += bd.playerModifier;
                    break;
                case BuffData.ModifierType.multiplicative:
                    actualStats *= bd.playerModifier;
                    break;
            }
        }
        actualStats *= multiplier;
        //update the PlayerCollector's radius
        collector.SetRadius(actualStats.magnet);
    }

    public void IncreaseExperience(int amount)
    {
        experience += amount;

        LevelUpChecker();
        UpdateExpBar();
    }
    
    void LevelUpChecker()
    {
        if(experience >= experienceCap)
        {
            level++;
            experience -= experienceCap;
            int experienceCapIncrease = 0;
            foreach(LevelRange range in levelRanges)
            {
                if (level >= range.startLevel && level <= range.endLevel)
                {
                    experienceCapIncrease = range.experienceCapIncrease;
                    break;
                }
            }
            experienceCap += experienceCapIncrease;
            UpdateLevelText();
            GameManager.instance.StartLevelUp();

            if (experience >= experienceCap) LevelUpChecker();
        }
    }

    public void UpdateExpBar()
    {
        if(expBar)
            expBar.fillAmount = (float)experience / experienceCap;

    }

    public void UpdateLevelText()
    {
               levelText.text = "LVL: " + level.ToString();
    }

    public override void TakeDamage(float dmg) 
    {
        Debug.Log($"Incoming: {dmg + actualStats.armor}, Armor: {actualStats.armor}");

        if (!isInvincible)
        {
            //take armor into account before dealing damage
            dmg -= actualStats.armor;
            if (dmg > 0)
            {
                //deal the damage
                CurrentHealth -= dmg;
                if (damageEffect) Destroy(Instantiate(damageEffect, transform.position, Quaternion.identity), 5f);

                if (CurrentHealth <= 0)
                {
                    Kill();
                }
            }
            else
            {
                //if there is blocked effect assigned, play it
                if (blockedEffect) Destroy(Instantiate(blockedEffect, transform.position, Quaternion.identity), 5f);
            }

            invincibilityTimer = invincibilityDuration;
            isInvincible = true;


            updateHealthBar();
        }
    }

    public void updateHealthBar()
    {
        healthBar.fillAmount = CurrentHealth / actualStats.maxHealth;
    }

    public override void Kill()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.GameOver();
        }
    }


    public override void RestoreHealth(float amount)
    {
        if (CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += amount;

            if(CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }

            updateHealthBar();
        }
    }

    void Recover()
    {
        if(CurrentHealth < actualStats.maxHealth)
        {
            CurrentHealth += Stats.recovery * Time.deltaTime;

            if (CurrentHealth > actualStats.maxHealth)
            {
                CurrentHealth = actualStats.maxHealth;
            }

            updateHealthBar();
        }
    }



}
