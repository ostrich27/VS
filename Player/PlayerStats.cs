using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
public class PlayerStats : MonoBehaviour
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

    float health;




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
    public int weaponIndex;
    public int passiveItemIndex;


    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TextMeshProUGUI levelText;


    PlayerAnimator playerAnimator;
    void Awake()
    {
            
        characterData = CharacterSelector.GetData();
        if(CharacterSelector.instance) CharacterSelector.instance.DestroySingleton();



        inventory = GetComponent<PlayerInventory>();
        collector = GetComponentInChildren<PlayerCollector>();

        baseStats = actualStats = characterData.stats;
        health = actualStats.maxHealth;
        collector.SetRadius(actualStats.magnet);

        playerAnimator = GetComponent<PlayerAnimator>();
        playerAnimator.SetAnimatorController(characterData.controller);
    }


    void Start()
    {
        //spawn the starting weapon
        inventory.Add(characterData.StartingWeapon);

        //initialize the experience cap as the first experience cap increase
        experienceCap = levelRanges[0].experienceCapIncrease;


        GameManager.instance.AssignChosenCharacterUI(characterData);


        updateHealthBar();
        UpdateLevelText();
        UpdateExpBar();
    }


    private void Update()
    {
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

    public void RecalculateStats()
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
        expBar.fillAmount = (float)experience / experienceCap;
    }

    public void UpdateLevelText()
    {
               levelText.text = "LVL: " + level.ToString();
    }

    public void TakeDamage(float dmg) 
    {
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

    public void Kill()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.GameOver();
        }
    }


    public void RestoreHealth(float amount)
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
