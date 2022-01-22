﻿namespace FuryCore.Interfaces;

using System;
using FuryCore.Models;
using StardewModdingAPI.Events;
using StardewValley.Menus;

/// <summary>
///     Custom Events raised by FuryCore.
/// </summary>
public interface IFuryEvents
{
    /// <summary>
    ///     Triggers when an <see cref="ItemGrabMenu" /> is constructed or when an the Active Menu switches to/from an
    ///     <see cref="ItemGrabMenu" />.
    /// </summary>
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged;

    /// <summary>
    ///     Triggers when a vanilla or custom <see cref="MenuComponent" /> is pressed on an <see cref="ItemGrabMenu" />.
    /// </summary>
    public event EventHandler<MenuComponentPressedEventArgs> MenuComponentPressed;

    /// <summary>
    ///     Triggers before an <see cref="ItemGrabMenu" /> renders. Anything drawn to the SpriteBatch will be above the
    ///     background but behind the menu.
    /// </summary>
    public event EventHandler<RenderedActiveMenuEventArgs> RenderedItemGrabMenu;

    /// <summary>
    ///     Triggers after an <see cref="ItemGrabMenu" /> renders. Anything drawn to the SpriteBatch will be above the menu but
    ///     behind the cursor and hovered text/items.
    /// </summary>
    public event EventHandler<RenderingActiveMenuEventArgs> RenderingItemGrabMenu;
}