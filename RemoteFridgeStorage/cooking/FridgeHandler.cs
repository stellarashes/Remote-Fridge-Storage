using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace RemoteFridgeStorage
{
    /// <summary>
    /// Takes care of adding and removing elements for crafting.
    /// </summary>
    public class FridgeHandler : InventoryHandler
    {
        public IList<Item> FridgeList
        {
            get => ItemsList;
            private set => ItemsList = value;
        }

        /// <summary>
        /// Is true when the _categorizeChests is loaded so that the icon can be moved.
        /// </summary>
        private readonly bool _categorizeChestsLoaded;

        /// <summary>
        /// When cooking skill is loaded so that the crafting menu wont be replaced.
        /// </summary>
        private readonly bool _cookingSkillLoaded;


        /// <summary>
        /// Creates a new handler for fridge items.
        /// </summary>
        /// <param name="textureFridge"></param>
        /// <param name="textureFridge2"></param>
        /// <param name="categorizeChestsLoaded"></param>
        /// <param name="cookingSkillLoaded"></param>
        public FridgeHandler(Texture2D textureFridge, Texture2D textureFridge2, bool categorizeChestsLoaded,
            bool cookingSkillLoaded) : base(textureFridge, textureFridge2)
        {
            _cookingSkillLoaded = cookingSkillLoaded;
            _categorizeChestsLoaded = categorizeChestsLoaded;
            FridgeList = new HandlerVirtualList(this);
        }

        /// <inheritdoc />
        protected override void UpdatePos()
        {
            var menu = Game1.activeClickableMenu;
            if (menu == null) return;

            var xOffset = 0.0;
            var yOffset = 1.0;
            if (_categorizeChestsLoaded)
            {
                xOffset = -1.0;
                yOffset = -0.25;
            }

            var xScaledOffset = (int) (xOffset * Game1.tileSize);
            var yScaledOffset = (int) (yOffset * Game1.tileSize);

            IconSelected.bounds = IconDeselected.bounds = new Rectangle(
                menu.xPositionOnScreen - 17 * Game1.pixelZoom + xScaledOffset,
                menu.yPositionOnScreen + yScaledOffset + Game1.pixelZoom * 5, 16 * Game1.pixelZoom,
                16 * Game1.pixelZoom);
        }

        /// <summary>
        /// Load all fridges.
        /// </summary>
        public override void AfterLoad()
        {
            //
            Chests.Clear();
            var farmHouse = Game1.getLocationFromName("farmHouse") as FarmHouse;
            foreach (var gameLocation in GetLocations())
            {
                foreach (var gameLocationObject in gameLocation.objects.Values)
                {
                    LoadChest(gameLocationObject, farmHouse);
                }
            }
        }

        private void LoadChest(Object gameLocationObject, FarmHouse farmHouse)
        {
            if (!(gameLocationObject is Chest chest)) return;

            if (chest.fridge.Value && chest != farmHouse?.fridge.Value)
            {
                Chests.Add(chest);
                chest.fridge.Value = false;
            }
        }

        /// <summary>
        /// Hacky way to store which chests are selected
        /// </summary>
        public override void BeforeSave()
        {
            foreach (var chest in Chests)
            {
                chest.fridge.Value = true;
            }
        }

        /// <summary>
        /// Reset the fridge booleans
        /// </summary>
        public override void AfterSave()
        {
            //Reset the fridge flag for all chests.
            foreach (var chest in Chests)
            {
                chest.fridge.Value = false;
            }
        }

        protected override void ChestOpenEvent()
        {
            FridgeList = new HandlerVirtualList(this);
            UpdatePos();
        }

        protected override void ChestCloseEvent()
        {
            FridgeList = new HandlerVirtualList(this);
        }

        /// <summary>
        /// Replace the menu 
        /// </summary>
        /// <param name="argEvents"></param>
        public override void LoadMenu(EventArgsClickableMenuChanged argEvents)
        {
            FridgeList = new HandlerVirtualList(this);
            if (!_cookingSkillLoaded || ModEntry.Instance.CookingSkillApi == null)
            {
                Game1.activeClickableMenu = new RemoteFridgeCraftingPage(argEvents.NewMenu, this);
            }
        }
    }
}