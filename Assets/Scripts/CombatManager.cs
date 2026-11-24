using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class CombatManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GiantManager giantManager;
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
        int damage = Mathf.Max(0, rollDie(playerController.playerWeapon.weaponDie) - rollDie(enemy.armor.armorDie));
        enemy.health -= damage;
        uiManager.SendMessage($"-{damage}", 100, Color.red, uiManager.enemyImage.transform.position.x, 0);
        uiManager.attackAnimation(true);
        await Awaitable.WaitForSecondsAsync(1.0f, destroyCancellationToken);

        if (enemy.health <= 0) { // checks to see if the enemy is dead
            uiManager.disableCombatUI();
            uiManager.playerWalkOffscreen();
            uiManager.characterDeath(uiManager.enemyImage);
            uiManager.SendMessage($"The Slumbering Giant is {giantManager.getGiantCompassDirection()}", 100, Color.white, Screen.width / 2, -Screen.height / 8);
            replenishMovement();
            return;
        }

        // enemy attack
        enemyAttack();

        giantManager.MoveGiant();
        //Debug.Log("Lets get this punk!");
    }

    // attempts to run from the enemy when you click the flee button
    void flee() 
    {
        int fleeDie = rollDie(20) + rollDie((playerController.maxHealth/playerController.health) * 10);
        if (fleeDie >= 15) 
        {
            uiManager.SendMessage($"You Escaped the {enemy.name}!", 100, Color.green, Screen.width / 2, -Screen.height / 8);
            uiManager.disableCombatUI();
            uiManager.playerWalkOffscreen();
            uiManager.enemyWalkOffscreen();
            replenishMovement();
        } else 
        {
            uiManager.SendMessage("You Aren't Able to Get Away!", 100, Color.red, Screen.width / 2, Screen.height / 8);
            enemyAttack();
        }

        giantManager.MoveGiant();
        //Debug.Log("RUN AWWAAAAY RUN AWAAAAY!!!");
    }

    // attempts to convince the enemy when you click the convince button
    void convince()
    {
        int fleeDie = rollDie(20) + rollDie(playerController.playerWeapon.weaponDie);
        if (fleeDie >= 20) 
        {
            //uiManager.SendMessage("You Befriended Them!", 100, Color.green, Screen.width / 2, Screen.height / 4);
            uiManager.SendMessage($"The Slumbering Giant is {giantManager.getGiantCompassDirection()}", 100, Color.white, Screen.width / 2, -Screen.height / 8);
            uiManager.disableCombatUI();
            uiManager.playerWalkOffscreen();
            uiManager.enemyWalkOffscreen();
            replenishMovement();
        } else 
        {
            uiManager.SendMessage("They Arent Friendly!", 100, Color.red, Screen.width / 2, Screen.height / 8);
            enemyAttack();
        }

        giantManager.MoveGiant();
        //Debug.Log("I dont want to fight!");
    }

    // the enemy attacks the player
    void enemyAttack() 
    {
        int damage = rollDie(enemy.weapon.weaponDie - playerController.playerArmor.armorDie);
        playerController.health -= damage;
        uiManager.SendMessage($"-{damage}", 100, Color.red, uiManager.playerImage.transform.position.x, 0);
        uiManager.attackAnimation(false);

        if (playerController.health <= 0) { // checks to see if the player is dead
            uiManager.characterDeath(uiManager.playerImage);
            uiManager.enemyWalkOffscreen();
            uiManager.disableCombatUI();
            return;
        }
    }
    
    // creates a list of random enemies to fight
    void initializeEnemyList()
    {
        Enemy slumberingGiant = new Enemy("Slumbering Giant", 10, 10, Gear.shortSword, Gear.leatherArmor); // keep the slumbering giant at index 0 of the list
        enemies.Add(slumberingGiant);
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
        enemy = new Enemy(enemies[Random.Range(1, enemies.Count)]);
        uiManager.resetEnemyImage();
    }

    public void GiantCombat()
    {
        enemy = new Enemy(enemies[0]);
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

