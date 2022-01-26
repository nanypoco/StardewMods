﻿namespace FuryCore.Events;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FuryCore.Enums;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ItemGrabMenuChanged : SortedEventHandler<ItemGrabMenuChangedEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemGrabMenuChanged" /> class.
    /// </summary>
    /// <param name="gameLoop"></param>
    /// <param name="services"></param>
    public ItemGrabMenuChanged(IGameLoopEvents gameLoop, ServiceCollection services)
    {
        ItemGrabMenuChanged.Instance ??= this;

        services.Lazy<HarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{ModEntry.ModUniqueId}.{nameof(ItemGrabMenuChanged)}";
                var ctorParams = new[]
                {
                    typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
                };

                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Constructor(typeof(ItemGrabMenu), ctorParams),
                    typeof(ItemGrabMenuChanged),
                    nameof(ItemGrabMenuChanged.ItemGrabMenu_constructor_postfix),
                    PatchType.Postfix);

                harmonyHelper.ApplyPatches(id);
            });

        gameLoop.UpdateTicked += this.OnUpdateTicked;
        gameLoop.UpdateTicking += this.OnUpdateTicking;
    }

    private static ItemGrabMenuChanged Instance { get; set; }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        ItemGrabMenuChanged.Instance.Menu = __instance;

        if (__instance is not { shippingBin: false, context: Chest { playerChest.Value: true } chest })
        {
            ItemGrabMenuChanged.Instance.InvokeAll(new(__instance, null, -1, false));
            return;
        }

        ItemGrabMenuChanged.Instance.InvokeAll(new(__instance, chest, Context.ScreenId, true));
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        this.InvokeIfMenuChanged();
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
    {
        this.InvokeIfMenuChanged();
    }

    private void InvokeIfMenuChanged()
    {
        var menu = Game1.activeClickableMenu;

        if (ReferenceEquals(this.Menu, menu))
        {
            return;
        }

        this.Menu = Game1.activeClickableMenu;
        if (this.Menu is not ItemGrabMenu { shippingBin: false, context: Chest { playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin } chest } itemGrabMenu)
        {
            this.InvokeAll(new(this.Menu as ItemGrabMenu, null, -1, false));
            return;
        }

        this.InvokeAll(new(itemGrabMenu, chest, Context.ScreenId, false));
    }
}