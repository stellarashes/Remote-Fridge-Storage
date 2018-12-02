using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Menus;

namespace RemoteFridgeStorage.crafting
{
  /// <summary>
  /// The methods in this class are copied from CraftingPage. The only difference is that every line with <code> this.cooking ? this.fridge() : (IList[Item>) null </code>
  /// is replaced with GetCraftingItems()
  /// All private values are accesed through reflection using properties in this class.
  /// </summary>
    public class RemoteCraftCraftingMenu : CraftingPage
    {
      private CraftingHandler _craftingHandler;

      public RemoteCraftCraftingMenu(IClickableMenu page, CraftingHandler craftingHandler) :
            base(page.xPositionOnScreen, page.yPositionOnScreen, page.width, page.height, false)
        {
          _craftingHandler = craftingHandler;
        }
      
      // Get tge oruvateFuelds
      private string _descriptionText => Util.GetField<string>(this, "descriptionText");
      private string _hoverText => Util.GetField<string>(this, "hoverText");
      private string _hoverTitle => Util.GetField<string>(this, "hoverTitle");
      private Item _hoverItem => Util.GetField<Item>(this, "hoverItem");
      private Item _lastCookingHover => Util.GetField<Item>(this, "lastCookingHover");
      private Item _heldItem
      {
        get => Util.GetField<Item>(this, "heldItem");
        set => Util.SetField<Item>(this,"heldItem",value);
      }

      private int _currentCraftingPage
      {
        get => Util.GetField<int>(this, "currentCraftingPage");
        set => Util.SetField<int>(this, "currentCraftingPage", value);
      }

      private CraftingRecipe _hoverRecipe=> Util.GetField<CraftingRecipe>(this, "hoverRecipe");
      private bool _cooking => Util.GetField<bool>(this, "cooking");


      private void _clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
      {
        Util.InvokeMethod(this, "clickCraftingRecipe", c, playSound);
      }
      
      private IList<Item> GetCraftingItems()
      {
        return ModEntry.Crafting();
      }

      
      // Frome here on are all the copied methods.
      
      
      public override void receiveLeftClick(int x, int y, bool playSound = true)
      {
        base.receiveLeftClick(x, y, true);
        this._heldItem = this.inventory.leftClick(x, y, this._heldItem, true);
        if (this.upButton != null && this.upButton.containsPoint(x, y) && this._currentCraftingPage > 0)
        {
          Game1.playSound("coin");
          this._currentCraftingPage = Math.Max(0, this._currentCraftingPage - 1);
          this.upButton.scale = this.upButton.baseScale;
          this.upButton.leftNeighborID = this.pagesOfCraftingRecipes[this._currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key.myID;
        }
        if (this.downButton != null && this.downButton.containsPoint(x, y) && this._currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
        {
          Game1.playSound("coin");
          this._currentCraftingPage = Math.Min(this.pagesOfCraftingRecipes.Count - 1, this._currentCraftingPage + 1);
          this.downButton.scale = this.downButton.baseScale;
          this.downButton.leftNeighborID = this.pagesOfCraftingRecipes[this._currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key.myID;
        }
        foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this._currentCraftingPage].Keys)
        {
          int num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
          for (int index = 0; index < num; ++index)
          {
            if (key.containsPoint(x, y) && !key.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this._currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(GetCraftingItems()))
              this._clickCraftingRecipe(key, index == 0);
          }
        }
        if (this.trashCan != null && this.trashCan.containsPoint(x, y) && (this._heldItem != null && this._heldItem.canBeTrashed()))
        {
          if (this._heldItem is StardewValley.Object && Game1.player.specialItems.Contains((int) ((NetFieldBase<int, NetInt>) (this._heldItem as StardewValley.Object).parentSheetIndex)))
            Game1.player.specialItems.Remove((int) ((NetFieldBase<int, NetInt>) (this._heldItem as StardewValley.Object).parentSheetIndex));
          this._heldItem = (Item) null;
          Game1.playSound("trashcan");
        }
        else
        {
          if (this._heldItem == null || this.isWithinBounds(x, y) || !this._heldItem.canBeTrashed())
            return;
          Game1.playSound("throwDownITem");
          Game1.createItemDebris(this._heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, (GameLocation) null, -1);
          this._heldItem = (Item) null;
        }
      }

      

      public override void receiveRightClick(int x, int y, bool playSound = true)
        {
          this._heldItem = this.inventory.rightClick(x, y, this._heldItem, true);
          foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this._currentCraftingPage].Keys)
          {
            if (key.containsPoint(x, y) && !key.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this._currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(GetCraftingItems()))
              this._clickCraftingRecipe(key, true);
          }
        }
      
      public override void draw(SpriteBatch b)
      {
        if (this._cooking)
          Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string) null, false, false);
        this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256, false);
        this.inventory.draw(b);
        if (this.trashCan != null)
        {
          this.trashCan.draw(b);
          b.Draw(Game1.mouseCursors, new Vector2((float) (this.trashCan.bounds.X + 60), (float) (this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(686, 256, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
        }
        b.End();
        b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState) null, (RasterizerState) null);
        foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this._currentCraftingPage].Keys)
        {
          if (key.hoverText.Equals("ghosted"))
            key.draw(b, Color.Black * 0.35f, 0.89f);
          else if (!this.pagesOfCraftingRecipes[this._currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(GetCraftingItems()))
          {
            key.draw(b, Color.LightGray * 0.4f, 0.89f);
          }
          else
          {
            key.draw(b);
            if (this.pagesOfCraftingRecipes[this._currentCraftingPage][key].numberProducedPerCraft > 1)
              NumberSprite.draw(this.pagesOfCraftingRecipes[this._currentCraftingPage][key].numberProducedPerCraft, b, new Vector2((float) (key.bounds.X + 64 - 2), (float) (key.bounds.Y + 64 - 2)), Color.Red, (float) (0.5 * ((double) key.scale / 4.0)), 0.97f, 1f, 0, 0);
          }
        }
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState) null, (RasterizerState) null);
        if (this._hoverItem != null)
          IClickableMenu.drawToolTip(b, this._hoverText, this._hoverTitle, this._hoverItem, this._heldItem != null, -1, 0, -1, -1, (CraftingRecipe) null, -1);
        else if (!string.IsNullOrEmpty(this._hoverText))
          IClickableMenu.drawHoverText(b, this._hoverText, Game1.smallFont, this._heldItem != null ? 64 : 0, this._heldItem != null ? 64 : 0, -1, (string) null, -1, (string[]) null, (Item) null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe) null);
        if (this._heldItem != null)
          this._heldItem.drawInMenu(b, new Vector2((float) (Game1.getOldMouseX() + 16), (float) (Game1.getOldMouseY() + 16)), 1f);
        base.draw(b);
        if (this.downButton != null && this._currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
          this.downButton.draw(b);
        if (this.upButton != null && this._currentCraftingPage > 0)
          this.upButton.draw(b);
        if (this._cooking)
          this.drawMouse(b);
        if (this._hoverRecipe == null)
          return;
        SpriteBatch b1 = b;
        string text = " ";
        SpriteFont smallFont = Game1.smallFont;
        int xOffset = this._heldItem != null ? 48 : 0;
        int yOffset = this._heldItem != null ? 48 : 0;
        int moneyAmountToDisplayAtBottom = -1;
        string displayName = this._hoverRecipe.DisplayName;
        int healAmountToDisplay = -1;
        string[] buffIconsToDisplay;
        if (this._cooking && this._lastCookingHover != null)
        {
          if (Game1.objectInformation[(int) ((NetFieldBase<int, NetInt>) (this._lastCookingHover as StardewValley.Object).parentSheetIndex)].Split('/').Length > 7)
          {
            buffIconsToDisplay = Game1.objectInformation[(int) ((NetFieldBase<int, NetInt>) (this._lastCookingHover as StardewValley.Object).parentSheetIndex)].Split('/')[7].Split(' ');
            goto label_32;
          }
        }
        buffIconsToDisplay = (string[]) null;
  label_32:
        Item lastCookingHover = this._lastCookingHover;
        int currencySymbol = 0;
        int extraItemToShowIndex = -1;
        int extraItemToShowAmount = -1;
        int overrideX = -1;
        int overrideY = -1;
        double num = 1.0;
        CraftingRecipe hoverRecipe = this._hoverRecipe;
        IClickableMenu.drawHoverText(b1, text, smallFont, xOffset, yOffset, moneyAmountToDisplayAtBottom, displayName, healAmountToDisplay, buffIconsToDisplay, lastCookingHover, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, overrideX, overrideY, (float) num, hoverRecipe);
      }
    }
}