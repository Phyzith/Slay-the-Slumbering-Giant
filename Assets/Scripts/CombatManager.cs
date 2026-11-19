using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class CombatManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;
    private List<Enemy> enemies = new List<Enemy>();
    public Enemy enemy;

    // bools associated with checking if something has happened
    private bool isMovementDepleted = false;

    // buttons
    [SerializeField] private Button attackButton;
    [SerializeField] private Button fleeButton;
    [SerializeField] private Button convinceButton;


    void Start()
    {
        initializeEnemyList();
        initializeCombat();
        attackButton.onClick.AddListener(attack);
        fleeButton.onClick.AddListener(flee);
        convinceButton.onClick.AddListener(convince);
    }

    void Update()
    {
        checkMovementDepleted();
    }

    void checkMovementDepleted() 
    {
        if (playerController.playerMovement <= 0)
        {
            if (!isMovementDepleted)
            {
                isMovementDepleted = true;
                uiManager.enableCombatUI();
            }
        }
        else
        {
            isMovementDepleted = false;
        }
    }

    int rollDie(int numSides) {return Random.Range(1, numSides + 1);} // rolls a dice of with numSides sides

    // attacks the enemy when you click the attack button
    async void attack() 
    {
        // player attack
        enemy.health -= Mathf.Max(0, rollDie(playerController.playerWeapon.weaponDie) - rollDie(enemy.armor.armorDie));
        uiManager.attackAnimation(true);
        Debug.Log("Lets get this punk!");
        await Awaitable.WaitForSecondsAsync(1.0f, destroyCancellationToken);
        if (enemy.health <= 0) { // checks to see if the enemy is dead
            uiManager.disableCombatUI();
            replenishMovement();
            return;
        }

        // enemy attack
        enemyAttack();
    }

    // attempts to run from the enemy when you click the flee button
    void flee() 
    {
        int fleeDie = rollDie(20) + rollDie((playerController.maxHealth/playerController.health) * 10);
        if (fleeDie >= 20) 
        {
            uiManager.disableCombatUI();
            replenishMovement();
        }

        Debug.Log("RUN AWWAAAAY RUN AWAAAAY!!!");
    }

    // attempts to convince the enemy when you click the convince button
    void convince()
    {
        int fleeDie = rollDie(20) + rollDie(playerController.playerWeapon.weaponDie);
        if (fleeDie >= 20) 
        {
            uiManager.disableCombatUI();
            replenishMovement();
        }

        Debug.Log("I dont want to fight man!");
    }

    void enemyAttack() 
    {
        playerController.health -= rollDie(enemy.weapon.weaponDie - playerController.playerArmor.armorDie);
        uiManager.attackAnimation(false);

        if (playerController.health <= 0) { // checks to see if the player is dead
            uiManager.disableCombatUI();
            return;
        }
    }
    
    // creates a list of random enemies to fight
    void initializeEnemyList()
    {
        Enemy fishMan = new Enemy("Fish Man", 3, 3, Gear.shortSword, Gear.noArmor);
        enemies.Add(fishMan);
        Enemy potMan = new Enemy("Pot Man", 3, 3, Gear.shortBow, Gear.leatherArmor);
        enemies.Add(potMan);
    }

    void initializeCombat() 
    {
        uiManager.createGearCard(playerController.playerWeapon.name, $"Deals 1 to {playerController.playerWeapon.weaponDie} Damage");
        uiManager.createGearCard(playerController.playerArmor.name, $"Protects against 1 to {playerController.playerArmor.armorDie} Damage");
    }

    public void setNewEnemy()
    {
        enemy = new Enemy(enemies[Random.Range(0, enemies.Count)]);
    }

    private void replenishMovement() {playerController.playerMovement = Mathf.Min(playerController.playerMovement += playerController.maxPlayerMovement, playerController.maxPlayerMovement);}
}

[System.Serializable]
public class Enemy
{
    public string name;
    public int maxHealth;
    public int health;
    public Weapon weapon;
    public Armor armor;

    public Enemy(string name, int health, int maxHealth, Weapon weapon, Armor armor) 
    {
        this.name = name;
        this.health = health;
        this.maxHealth = maxHealth;
        this.weapon = weapon;
        this.armor = armor;
    }

    public Enemy(Enemy otherEnemy) 
    {
        this.name = otherEnemy.name;
        this.health = otherEnemy.health;
        this.maxHealth = otherEnemy.maxHealth;
        this.weapon = otherEnemy.weapon;
        this.armor = otherEnemy.armor;
    }
}

