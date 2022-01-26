﻿namespace FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Extensions;
using FuryCore.Attributes;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IMenuComponents" />
[FuryCoreService(true)]
internal class MenuComponents : IMenuComponents, IService
{
    private readonly PerScreen<List<MenuComponent>> _components = new(() => new());
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<bool> _refreshComponents = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponents" /> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public MenuComponents(IModHelper helper, ServiceCollection services)
    {
        MenuComponents.Instance = this;
        this.Helper = helper;

        services.Lazy<CustomEvents>(
            events =>
            {
                events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
                events.RenderedItemGrabMenu += this.OnRenderedItemGrabMenu;
                events.RenderingItemGrabMenu += this.OnRenderingItemGrabMenu;
            });

        services.Lazy<HarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{ModEntry.ModUniqueId}.{nameof(MenuComponents)}";

                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                    typeof(MenuComponents),
                    nameof(MenuComponents.ItemGrabMenu_RepositionSideButtons_prefix));

                harmonyHelper.ApplyPatches(id);
            });

        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
    }

    /// <inheritdoc />
    public List<MenuComponent> Components
    {
        get => this._components.Value;
    }

    /// <inheritdoc />
    public ItemGrabMenu Menu
    {
        get => this._menu.Value;
        private set => this._menu.Value = value;
    }

    private static MenuComponents Instance { get; set; }

    private IModHelper Helper { get; }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private bool RefreshComponents
    {
        get => this._refreshComponents.Value;
        set => this._refreshComponents.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
    {
        MenuComponents.Instance.RepositionSideButtons(__instance);
        return false;
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        if (!ReferenceEquals(this.Menu, Game1.activeClickableMenu))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var component in this.Components)
        {
            component.TryHover(x, y, 0.25f);

            if (component.Component?.containsPoint(x, y) == true)
            {
                this.HoverText = component.HoverText;
            }
        }
    }

    [SortedEventPriority(EventPriority.High + 1000)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu?.IsPlayerChestMenu(out _) == true
            ? e.ItemGrabMenu
            : null;

        this.Components.Clear();

        if (this.Menu is null)
        {
            return;
        }

        // Add vanilla components
        this.Components.AddRange(
            Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>()
                .Select(componentType => new MenuComponent(this.Menu, componentType))
                .Where(component => component.Component is not null)
                .OrderBy(component => component.Component.bounds.X)
                .ThenBy(component => component.Component.bounds.Y));
        this.RefreshComponents = true;
    }

    private void OnRenderedItemGrabMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        foreach (var component in this.Components.Where(component => component.IsCustom && component.Area is not ComponentArea.Bottom))
        {
            component.Draw(e.SpriteBatch);
        }

        if (string.IsNullOrWhiteSpace(this.Menu.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
        {
            this.Menu.hoverText = this._hoverText.Value;
        }
    }

    private void OnRenderingItemGrabMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        if (this.RefreshComponents && this.Menu is not null)
        {
            foreach (var component in this.Components.Where(component => component.Component is null).ToList())
            {
                this.Components.Remove(component);
            }

            foreach (var component in this.Components.Where(component => component.IsCustom))
            {
                this.Menu.allClickableComponents.Add(component.Component);
            }

            this.RepositionSideButtons(this.Menu);
            this.RefreshComponents = false;
        }

        foreach (var component in this.Components.Where(component => component.IsCustom && component.Area is ComponentArea.Bottom))
        {
            component.Draw(e.SpriteBatch);
        }
    }

    private void RepositionSideButtons(IClickableMenu menu)
    {
        if (!ReferenceEquals(this.Menu, menu))
        {
            return;
        }

        var sideComponents = this.Components.Where(component => component.Area is ComponentArea.Right && component.Component is not null).ToList();
        var stepSize = sideComponents.Count >= 4 ? 72 : 80;
        MenuComponent previousComponent = null;
        foreach (var (component, index) in sideComponents.AsEnumerable().Reverse().Select((component, index) => (component, index)))
        {
            if (previousComponent is not null)
            {
                previousComponent.Component.upNeighborID = component.Id;
                component.Component.downNeighborID = previousComponent.Id;
            }

            component.Component.bounds.X = menu.xPositionOnScreen + menu.width;
            component.Component.bounds.Y = menu.yPositionOnScreen + (menu.height / 3) - 64 - (stepSize * index);
            previousComponent = component;
        }
    }
}