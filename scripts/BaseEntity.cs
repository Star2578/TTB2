using System;
using Godot;

public partial class BaseEntity : CharacterBody2D
{
    public string name { get; }
    public string description { get; }
    private int maxHealth;
    private int health;
    private int maxMana;
    private int mana;
    private int maxStamina;
    private int stamina;

    public BaseEntity() : this("Placeholder Name", "Placeholder Description") { }

    public BaseEntity(string name, string description)
    {
        this.name = name;
        this.description = description;

        maxHealth = 1;
        health = maxHealth;
        maxMana = 1;
        mana = maxMana;
        maxStamina = 1;
        stamina = maxStamina;
    }

    #region Getter Setter

    public int MaxHealth
    {
        get { return maxHealth; }
        set
        {
            double ratio = (double)health / maxHealth;
            maxHealth = Math.Max(value, 1);

            health = (int)(maxHealth * ratio);
        }
    }
    public int Health
    {
        get { return health; }
        set
        {
            health = value;

            // health can never be below 0 and never above maxHealth
            health = Mathf.Min(Math.Max(health, 0), maxHealth);

            if (health <= 0) OnDeath();
        }
    }

    public int MaxMana
    {
        get { return maxMana; }
        set
        {
            double ratio = (double)mana / maxMana;
            maxMana = Math.Max(value, 1);

            mana = (int)(maxMana * ratio);
        }
    }
    public int Mana
    {
        get { return mana; }
        set
        {
            mana = value;

            // mana can never be below 0 and never above maxMana
            mana = Mathf.Min(Mathf.Max(mana, 0), maxMana);
        }
    }

    public int MaxStamina
    {
        get { return maxStamina; }
        set
        {
            double ratio = (double)stamina / maxStamina;
            maxStamina = Math.Max(value, 1);

            stamina = (int)(maxStamina * ratio);
        }
    }
    public int Stamina
    {
        get { return stamina; }
        set
        {
            stamina = value;

            // mana can never be below 0 and never above maxMana
            stamina = Mathf.Min(Mathf.Max(stamina, 0), maxStamina);
        }
    }

    #endregion

    // Method to call after the entity died
    public virtual void OnDeath()
    {
        GD.Print(name + " has died");
    }
}