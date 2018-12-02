using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RemoteFridgeStorage
{
    public abstract class InventoryHandler
    {
        protected IList<Item> ItemsList { get; set; }
        
        /// <summary>
        /// The chests that are included for considering the 
        /// </summary>
        public HashSet<Chest> Chests { get; }

        /// <summary>
        /// Texture button state included.
        /// </summary>
        protected readonly ClickableTextureComponent IconSelected;

        /// <summary>
        /// Texture button state excluded (default state)
        /// </summary>
        protected readonly ClickableTextureComponent IconDeselected;

        /// <summary>
        /// If the chest is currently open.
        /// </summary>
        private bool _opened;

        /// <summary>
        /// Creates a new InventoryHandler
        /// </summary>
        /// <param name="iconSelected">The icon to indicate that the current chest is included</param>
        /// <param name="iconDeselected">The icon to indicate that the current chest is excluded</param>
        protected InventoryHandler(Texture2D iconSelected, Texture2D iconDeselected)
        {
            _opened = false;
            IconSelected = new ClickableTextureComponent(Rectangle.Empty, iconSelected, Rectangle.Empty, 1f);
            IconDeselected = new ClickableTextureComponent(Rectangle.Empty, iconDeselected, Rectangle.Empty, 1f);
            Chests = new HashSet<Chest>();
        }

        /// <summary>
        /// Draw the fridge button
        /// </summary>
        public void DrawIcon()
        {
            var openChest = GetOpenChest();
            if (openChest == null) return;

            var farmHouse = Game1.getLocationFromName("farmHouse") as FarmHouse;

            if (openChest == farmHouse?.fridge.Value || Game1.activeClickableMenu == null ||
                !openChest.playerChest.Value) return;

            UpdatePos();

            if (Chests.Contains(openChest))
                IconSelected.draw(Game1.spriteBatch);
            else
                IconDeselected.draw(Game1.spriteBatch);

            Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()),
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero,
                4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Updates the position of the icons.
        /// </summary>
        protected abstract void UpdatePos();


        /// <summary>
        /// Handle the click event if it was on the icon.
        /// </summary>
        /// <param name="eventArgsInput"></param>
        public void HandleClick(EventArgsInput eventArgsInput)
        {
            var chest = GetOpenChest();
            if (chest == null) return;

            var screenPixels = eventArgsInput.Cursor.ScreenPixels;

            if (!IconSelected.containsPoint((int) screenPixels.X, (int) screenPixels.Y)) return;

            Game1.playSound("smallSelect");

            if (Chests.Contains(chest))
            {
                Chests.Remove(chest);
            }
            else
            {
                Chests.Add(chest);
            }
        }

        public abstract void AfterSave();
        public abstract void BeforeSave();
        public abstract void AfterLoad();
        protected abstract void ChestOpenEvent();
        protected abstract void ChestCloseEvent();

        /// <summary>
        /// Listen to ticks update to determine when a chest opens or closes.
        /// </summary>
        public void Game_Update()
        {
            var chest = GetOpenChest();
            if (chest == null && _opened)
            {
                ChestCloseEvent();
            }

            if (chest != null && !_opened)
            {
                ChestOpenEvent();
            }

            _opened = chest != null;
        }


        /// <summary>
        /// Gets all the locations of the world
        /// </summary>
        /// <returns>All the locations in an enumeratable</returns>
        protected static IEnumerable<GameLocation> GetLocations()
        {
            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }
        
        /// <summary>
        /// Gets the chest that is currently open or null if no chest is open.
        /// </summary>
        /// <returns>The chest that is open</returns>
        protected static Chest GetOpenChest()
        {
            if (Game1.activeClickableMenu == null)
                return null;

            if (!(Game1.activeClickableMenu is ItemGrabMenu)) return null;

            var menu = (ItemGrabMenu) Game1.activeClickableMenu;
            if (menu.behaviorOnItemGrab?.Target is Chest chest)
                return chest;

            return null;
        }


        public abstract void LoadMenu(EventArgsClickableMenuChanged argEvents);
    }
}