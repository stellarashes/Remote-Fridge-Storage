using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using RemoteFridgeStorage.apis;
using RemoteFridgeStorage.crafting;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace RemoteFridgeStorage
{
    /// <summary>The mod entry point.</summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModEntry : Mod
    {
        public static ModEntry Instance;

        private bool _displayedIsFridge;
        private bool _displayedIsCraft;
        private bool _cookingSkillLoaded;
        private FridgeHandler _fridgeHandler;
        private HarmonyInstance _harmony;
        private CraftingHandler _craftingHandler;

        public ICookingSkillApi CookingSkillApi { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;


            var fridgeSelected = helper.Content.Load<Texture2D>("assets/fridge.png");
            var fridgeDeselected = helper.Content.Load<Texture2D>("assets/fridge2.png");
            
            var craftSelected = helper.Content.Load<Texture2D>("assets/craft.png");
            var craftDeselected = helper.Content.Load<Texture2D>("assets/craft2.png");

            var categorizeChestsLoaded = helper.ModRegistry.IsLoaded("CategorizeChests") ||
                                         helper.ModRegistry.IsLoaded("aEnigma.ConvenientChests");
            _cookingSkillLoaded = helper.ModRegistry.IsLoaded("spacechase0.CookingSkill");


            if (_cookingSkillLoaded) Monitor.Log("Cooking skill is loaded on game start try to hook into the api");
            Harmony();

            _fridgeHandler = new FridgeHandler(fridgeSelected, fridgeDeselected, categorizeChestsLoaded,
                _cookingSkillLoaded);
            _craftingHandler = new CraftingHandler(craftSelected, craftDeselected, categorizeChestsLoaded);

            MenuEvents.MenuChanged += MenuChanged_Event;
            InputEvents.ButtonPressed += Button_Pressed_Event;
            GameEvents.FirstUpdateTick += Game_FirstTick;
            GraphicsEvents.OnPostRenderGuiEvent += Draw;

            SaveEvents.AfterLoad += AfterLoad;
            SaveEvents.BeforeSave += BeforeSave;
            SaveEvents.AfterSave += AfterSave;
            GameEvents.UpdateTick += Game_Update;
        }

        private void Game_FirstTick(object sender, EventArgs e)
        {
            if (!_cookingSkillLoaded) return;
            
            CookingSkillApi = Helper.ModRegistry.GetApi<ICookingSkillApi>("spacechase0.CookingSkill");

            if (CookingSkillApi == null)
            {
                Monitor.Log(
                    "Could not load CookingSkill API, mods might not work correctly, are you using the patched version of cooking skills https://github.com/SoapStuff/CookingSkill/releases?",
                    LogLevel.Warn);
            }
            else
            {
                CookingSkillApi.setFridgeFunction(Fridge);
                Monitor.Log("Successfully hooked into the cooking skill API!", LogLevel.Info);
            }
        }

        private void Harmony()
        {
            _harmony = HarmonyInstance.Create("productions.EternalSoap.RemoteFridgeStorage");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Game_Update(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;

            _fridgeHandler.Game_Update();
            _craftingHandler.Game_Update();
        }

        private void AfterSave(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _fridgeHandler.AfterSave();
            _craftingHandler.AfterSave();
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _fridgeHandler.BeforeSave();
            _craftingHandler.BeforeSave();
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _fridgeHandler.AfterLoad();
            _craftingHandler.AfterLoad();
        }


        private void Draw(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady) return;
            _fridgeHandler.DrawIcon();
            _craftingHandler.DrawIcon();
        }

        private void Button_Pressed_Event(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady) return;

            if (e.Button == SButton.MouseLeft)
            {
                _fridgeHandler.HandleClick(e);
                _craftingHandler.HandleClick(e);
            }
        }

        /// <summary>
        /// If the opened menu was a crafting menu, call the handler to load the menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuChanged_Event(object sender, EventArgsClickableMenuChanged e)
        {
            if (!Context.IsWorldReady || e.NewMenu == e.PriorMenu) return;
            //Replace menu if the new menu has the attribute cooking set to true and the new menu is not my crafting page.
            if (e.NewMenu != null &&
                Helper.Reflection.GetField<bool>(e.NewMenu, "cooking", false) != null &&
                Helper.Reflection.GetField<bool>(e.NewMenu, "cooking").GetValue() &&
                !(e.NewMenu is RemoteFridgeCraftingPage))
            {
                _displayedIsFridge = true;
                _displayedIsCraft = false;
                _fridgeHandler.LoadMenu(e);
            } else if (e.NewMenu is GameMenu && !(e.NewMenu is RemoteCraftCraftingMenu))
            {
                _displayedIsFridge = true;
                _displayedIsCraft = false;
                _craftingHandler.LoadMenu(e);
            }
        }

        /// <summary>
        /// Return the list used for the fridge items.
        /// </summary>
        /// <returns></returns>
        protected virtual IList<Item> FridgeImpl()
        {
            return _fridgeHandler.FridgeList;
        }
        
        /// <summary>
        /// Return the list used for the crafting inventory items.
        /// </summary>
        /// <returns></returns>
        protected virtual IList<Item> CraftingImpl()
        {
            return _craftingHandler.CrafingItems;
        }

        /// <summary>
        /// Calls the FridgeImpl method on the ModEntry instance.
        /// </summary>
        /// <returns></returns>
        public static IList<Item> Fridge()
        {
            return Instance.FridgeImpl();
        }

        /// <summary>
        /// Calls the CraftingImpl method on the ModEntry instance.
        /// </summary>
        /// <returns></returns>
        public static IList<Item> Crafting()
        {
            return Instance.CraftingImpl();
        }


        public static IList<Item> Items()
        {
            return Instance.ItemsImpl();
        }

        private IList<Item> ItemsImpl()
        {
            if (_displayedIsCraft) return CraftingImpl();
            if (_displayedIsFridge) return FridgeImpl();
            return null;
        }
    }
}