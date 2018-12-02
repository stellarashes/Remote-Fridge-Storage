using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace RemoteFridgeStorage.crafting
{
    public class CraftingHandler : InventoryHandler
    {
        private bool _categorizeChestsLoaded;

        public CraftingHandler(Texture2D iconSelected, Texture2D iconDeselected, bool categorizeChestsLoaded) : base(
            iconSelected, iconDeselected)
        {
            _categorizeChestsLoaded = categorizeChestsLoaded;
        }

        public IList<Item> CrafingItems
        {
            get => ItemsList;
            set => ItemsList = value;
        }

        protected override void UpdatePos()
        {
            var menu = Game1.activeClickableMenu;
            if (menu == null) return;

            var xOffset = -1.0;
            var yOffset = 1.0;
            if (_categorizeChestsLoaded)
            {
                xOffset = -2.0;
                yOffset = -0.25;
            }

            var xScaledOffset = (int) (xOffset * Game1.tileSize);
            var yScaledOffset = (int) (yOffset * Game1.tileSize);

            IconSelected.bounds = IconDeselected.bounds = new Rectangle(
                menu.xPositionOnScreen - 17 * Game1.pixelZoom + xScaledOffset,
                menu.yPositionOnScreen + yScaledOffset + Game1.pixelZoom * 5, 16 * Game1.pixelZoom,
                16 * Game1.pixelZoom);
        }

        public override void AfterSave()
        {
//            throw new System.NotImplementedException();
        }

        public override void BeforeSave()
        {
//            throw new System.NotImplementedException();
        }

        public override void AfterLoad()
        {
//            throw new System.NotImplementedException();
        }

        protected override void ChestOpenEvent()
        {
            CrafingItems = new HandlerVirtualList(this);
            UpdatePos();
        }

        protected override void ChestCloseEvent()
        {
            CrafingItems = new HandlerVirtualList(this);
        }

        /// <summary>
        /// Replace the menu 
        /// </summary>
        /// <param name="argEvents"></param>
        public override void LoadMenu(EventArgsClickableMenuChanged argEvents)
        {
            ModEntry.Instance.Monitor.Log("Replaced CraftingPage");
            CrafingItems = new HandlerVirtualList(this);
            var menu = Game1.activeClickableMenu as GameMenu;
            var menuPages = Util.GetField<List<IClickableMenu>>(menu, "pages");
            menuPages[GameMenu.craftingTab] = new RemoteCraftCraftingMenu(menuPages[GameMenu.craftingTab], this);
        }
    }
}