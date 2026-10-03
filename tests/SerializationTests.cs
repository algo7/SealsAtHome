using System.Linq;
using System.Reflection;
using SealsAtHome;

internal static partial class Tests
{
    private static FieldInfo F(System.Type type, string name) =>
        type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static void Test_Serialization_PublicFieldsAreSerialized()
    {
        True(UnitySerialization.IsSerialized(F(typeof(Character), "m_name")), "Character.m_name (public string)");
        True(UnitySerialization.IsSerialized(F(typeof(Character), "m_eye")), "Character.m_eye (public Transform)");
        True(UnitySerialization.IsSerialized(F(typeof(BaseAI), "m_viewRange")), "BaseAI.m_viewRange (public float)");
    }

    private static void Test_Serialization_RuntimeFieldsAndDelegatesAreNot()
    {
        False(UnitySerialization.IsSerialized(F(typeof(Character), "m_nview")), "Character.m_nview (protected, set in Awake)");
        False(UnitySerialization.IsSerialized(F(typeof(Character), "m_tamed")), "Character.m_tamed (private)");
        False(UnitySerialization.IsSerialized(F(typeof(Character), "m_onDeath")), "Character.m_onDeath (public delegate)");
    }

    private static void Test_Serialization_FieldsStopAtTheGivenType()
    {
        var baseAi = UnitySerialization.SerializedFields(typeof(BaseAI)).Select(f => f.Name).ToList();
        True(baseAi.Contains("m_viewRange") && baseAi.Contains("m_idleSound"), "BaseAI's own settings");
        False(baseAi.Contains("m_timeToSafe"), "not AnimalAI's");
        var animal = UnitySerialization.SerializedFields(typeof(AnimalAI)).Select(f => f.Name).ToList();
        True(animal.Contains("m_timeToSafe") && animal.Contains("m_viewRange"), "AnimalAI: its own fields and BaseAI's");
        var character = UnitySerialization.SerializedFields(typeof(Character)).Select(f => f.Name).ToList();
        True(character.Contains("m_name") && !character.Contains("m_defaultItems"), "Character's, not Humanoid's");
        Eq(character.Count, character.Distinct().Count(), "no field twice");
    }
}
