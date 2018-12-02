using StardewValley;

namespace RemoteFridgeStorage
{
    public static class Util
    {
        public static T GetField<T>(object instance, string field)
        {
            return ModEntry.Instance.Helper.Reflection.GetField<T>(instance, field, false).GetValue();
        }
        
        public static void SetField<T>(object instance, string field, T value)
        {
            ModEntry.Instance.Helper.Reflection.GetField<T>(instance, field, false).SetValue(value);
        }

        public static void InvokeMethod(object instance, string method, params object[] arguments)
        {
            var clickcraftingrecipe = "clickCraftingRecipe";
            ModEntry.Instance.Helper.Reflection.GetMethod(instance,method).Invoke(arguments);
        }
    }
}