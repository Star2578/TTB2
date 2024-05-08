using GdMUT;
using Godot;

public class Test_BaseEntity
{
#if TOOLS


    [CSTestFunction]
    public static Result TestContructor()
    {
        BaseEntity baseEntity = new BaseEntity("Test", "Valhalla");

        if (baseEntity.name != "Test" || baseEntity.description != "Valhalla")
            return new Result(false, "Constructor did not set properties correctly.");

        return new Result(true, "Passed, you did a great job!");
    }
    [CSTestFunction]
    public static Result TestHealthSystem()
    {
        BaseEntity baseEntity = new BaseEntity { MaxHealth = 200 };

        #region Health
        // ** Health could never be below 0
        // ** Health could never be above MaxHealth

        // Test setting health within valid range
        baseEntity.Health = 100;
        if (baseEntity.Health != 100) return new Result(false, "Health should be 100, but the value is " + baseEntity.Health);

        // Test setting health below 0
        baseEntity.Health = -50;
        if (baseEntity.Health != 0) return new Result(false, "Health should be 0, but the value is " + baseEntity.Health);

        // Test setting health above MaxHealth
        baseEntity.Health = 250;
        if (baseEntity.Health != 200) return new Result(false, "Health should be 200, but the value is " + baseEntity.Health);

        #endregion

        #region MaxHealth
        // ** MaxHealth could never be below 1
        // ** Health will always equal to MaxHealth if Health/MaxHealth ratio is 1 before update MaxHealth

        // Test setting MaxHealth within valid range
        baseEntity.MaxHealth = 1000;
        if (baseEntity.MaxHealth != 1000) return new Result(false, "MaxHealth should be 1000, but the value is " + baseEntity.MaxHealth);

        // Test setting MaxHealth below 1
        baseEntity.MaxHealth = -10;
        if (baseEntity.MaxHealth != 1) return new Result(false, "MaxHealth should be 1, but the value is " + baseEntity.MaxHealth);

        #endregion

        return new Result(true, "Passed, you did a great job!");
    }

    [CSTestFunction]
    public static Result TestManaSystem()
    {
        BaseEntity baseEntity = new BaseEntity();

        #region Mana
        // ** Mana can never be below 0 and never above MaxMana

        baseEntity.MaxMana = 100;

        // Test setting mana within valid range
        baseEntity.Mana = 50;
        if (baseEntity.Mana != 50) return new Result(false, "Mana should be 50, but the value of Mana is " + baseEntity.Mana);

        // Test setting mana below 0
        baseEntity.Mana = -10;
        if (baseEntity.Mana != 0) return new Result(false, "Mana should be 0, but the value of Mana is " + baseEntity.Mana);

        // Test setting mana above MaxMana
        baseEntity.Mana = 150;
        if (baseEntity.Mana != 100) return new Result(false, "Mana should be 100, but the value of Mana is " + baseEntity.Mana);

        #endregion

        return new Result(true, "Passed, you did a great job!");
    }

    [CSTestFunction]
    public static Result TestStaminaSystem()
    {
        BaseEntity baseEntity = new BaseEntity();

        #region Stamina
        // ** Stamina can never be below 0 and never above MaxStamina

        baseEntity.MaxStamina = 200;

        // Test setting stamina within valid range
        baseEntity.Stamina = 100;
        if (baseEntity.Stamina != 100) return new Result(false, "Stamina should be 100, but the value of Stamina is " + baseEntity.Stamina);

        // Test setting stamina below 0
        baseEntity.Stamina = -50;
        if (baseEntity.Stamina != 0) return new Result(false, "Stamina should be 0, but the value of Stamina is " + baseEntity.Stamina);

        // Test setting stamina above MaxStamina
        baseEntity.Stamina = 250;
        if (baseEntity.Stamina != 200) return new Result(false, "Stamina should be 200, but the value of Stamina is " + baseEntity.Stamina);

        #endregion

        return new Result(true, "Passed, you did a great job!");
    }

#endif
}
