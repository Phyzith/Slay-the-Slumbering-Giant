using UnityEngine;

public class Gear
{
    // Weapons
    public static readonly Weapon noWeapon = new Weapon("Nothing", 0);
    public static readonly Weapon shortSword = new Weapon("Short Sword", 6);
    public static readonly Weapon shortBow = new Weapon("Short Bow", 6);

    // Armor
    public static readonly Armor noArmor = new Armor("Nothing", 0);
    public static readonly Armor leatherArmor = new Armor("Leather Armor", 6);
}

[System.Serializable]
public class Weapon
{
    public string name;
    public int weaponDie;
    
    public Weapon(string name, int weaponDie) 
    {
        this.name = name;
        this.weaponDie = weaponDie;
    }

    public Weapon(Weapon otherEnemy) 
    {
        this.name = otherEnemy.name;
        this.weaponDie = otherEnemy.weaponDie;
    }
}

[System.Serializable]
public class Armor
{
    public string name;
    public int armorDie;
    
    public Armor(string name, int armorDie) 
    {
        this.name = name;
        this.armorDie = armorDie;
    }

    public Armor(Armor otherEnemy) 
    {
        this.name = otherEnemy.name;
        this.armorDie = otherEnemy.armorDie;
    }
}
