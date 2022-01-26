﻿namespace BetterChests.Models;

using System.Collections.Generic;
using System.Linq;
using BetterChests.Enums;
using BetterChests.Interfaces;
using FuryCore.Helpers;

/// <inheritdoc cref="BetterChests.Interfaces.IChestModel" />
internal class ChestModel : IChestModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChestModel"/> class.
    /// </summary>
    /// <param name="configData">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="chestData">The <see cref="IChestData" /> for options set by the player.</param>
    public ChestModel(IConfigModel configData, IChestData chestData)
    {
        this.Config = configData;
        this.Data = chestData;
    }

    // ****************************************************************************************
    // General

    /// <inheritdoc/>
    public ItemMatcher ItemMatcherByType { get; } = new(true);

    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get
        {
            if (this.Data.CarryChest != FeatureOption.Default)
            {
                return this.Data.CarryChest;
            }

            return this.Config.CarryChest != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CategorizeChest
    {
        get
        {
            if (this.Data.CategorizeChest != FeatureOption.Default)
            {
                return this.Data.CategorizeChest;
            }

            return this.Config.CategorizeChest != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.CategorizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get
        {
            if (this.Data.ChestMenuTabs != FeatureOption.Default)
            {
                return this.Data.ChestMenuTabs;
            }

            return this.Config.ChestMenuTabs != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.ChestMenuTabs = value;
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get
        {
            if (this.Data.CollectItems != FeatureOption.Default)
            {
                return this.Data.CollectItems;
            }

            return this.Config.CollectItems != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.CollectItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get
        {
            if (this.Data.CraftFromChest != FeatureOptionRange.Default)
            {
                return this.Data.CraftFromChest;
            }

            return this.Config.CraftFromChest == FeatureOptionRange.Default ? FeatureOptionRange.Location : this.Config.CraftFromChest;
        }
        set => this.Data.CraftFromChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get
        {
            if (this.Data.CustomColorPicker != FeatureOption.Default)
            {
                return this.Data.CustomColorPicker;
            }

            return this.Config.CustomColorPicker != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.CustomColorPicker = value;
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get
        {
            if (this.Data.FilterItems != FeatureOption.Default)
            {
                return this.Data.FilterItems;
            }

            return this.Config.FilterItems != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.FilterItems = value;
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get
        {
            if (this.Data.OpenHeldChest != FeatureOption.Default)
            {
                return this.Data.OpenHeldChest;
            }

            return this.Config.OpenHeldChest != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get
        {
            if (this.Data.ResizeChest != FeatureOption.Default)
            {
                return this.Data.ResizeChest;
            }

            return this.Config.ResizeChest != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.ResizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get
        {
            if (this.Data.ResizeChestMenu != FeatureOption.Default)
            {
                return this.Data.ResizeChestMenu;
            }

            return this.Config.ResizeChestMenu != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.ResizeChestMenu = value;
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get
        {
            if (this.Data.SearchItems != FeatureOption.Default)
            {
                return this.Data.SearchItems;
            }

            return this.Config.SearchItems != FeatureOption.Disabled ? FeatureOption.Enabled : FeatureOption.Disabled;
        }
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get
        {
            if (this.Data.StashToChest != FeatureOptionRange.Default)
            {
                return this.Data.StashToChest;
            }

            return this.Config.StashToChest == FeatureOptionRange.Default ? FeatureOptionRange.Location : this.Config.CraftFromChest;
        }
        set => this.Data.StashToChest = value;
    }

    // ****************************************************************************************
    // Feature Options

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get
        {
            if (this.Data.CraftFromChestDistance != 0)
            {
                return this.Data.CraftFromChestDistance;
            }

            return this.Config.CraftFromChestDistance == 0 ? -1 : this.Config.CraftFromChestDistance;
        }
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.Data.FilterItemsList.Any() ? this.Data.FilterItemsList : this.Config.FilterItemsList;
        set => this.Data.FilterItemsList = value;
    }

    /// <inheritdoc/>
    public bool FillStacks
    {
        get => this.Data.FillStacks;
        set => this.Data.FillStacks = value;
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get
        {
            if (this.Data.ResizeChestCapacity != 0)
            {
                return this.Data.ResizeChestCapacity;
            }

            return this.Config.ResizeChestCapacity == 0 ? 60 : this.Config.ResizeChestCapacity;
        }
        set => this.Data.ResizeChestCapacity = value;
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get
        {
            if (this.Data.ResizeChestMenuRows != 0)
            {
                return this.Data.ResizeChestMenuRows;
            }

            return this.Config.ResizeChestMenuRows == 0 ? 5 : this.Config.ResizeChestCapacity;
        }
        set => this.Data.ResizeChestMenuRows = value;
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get
        {
            if (this.Data.StashToChestDistance != 0)
            {
                return this.Data.StashToChestDistance;
            }

            return this.Config.StashToChestDistance == 0 ? -1 : this.Config.StashToChestDistance;
        }
        set => this.Data.StashToChestDistance = value;
    }

    private IConfigModel Config { get; }

    private IChestData Data { get; }
}