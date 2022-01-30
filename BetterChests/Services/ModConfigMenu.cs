﻿namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using FuryCore.Enums;
using FuryCore.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;

/// <inheritdoc cref="FuryCore.Interfaces.IModService" />
internal class ModConfigMenu : IModService, IModConfigMenu
{
    private const string CraftablesData = "Data/BigCraftablesInformation";

    /// <summary>
    /// Initializes a new instance of the <see cref="ModConfigMenu"/> class.
    /// </summary>
    /// <param name="chestData">The <see cref="IChestData" /> configured for each chest.</param>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    public ModConfigMenu(Dictionary<string, ChestData> chestData, IConfigModel config, IModHelper helper, IManifest manifest)
    {
        this.ChestData = chestData;
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private Dictionary<string, ChestData> ChestData { get; }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IConfigModel Config { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private IEnumerable<string[]> Craftables
    {
        get => this.Helper.Content.Load<Dictionary<string, string>>(ModConfigMenu.CraftablesData, ContentSource.GameContent).Values.Select(info => info.Split('/'));
    }

    /// <inheritdoc/>
    public void ChestConfig(IManifest manifest, IChestData config)
    {
        this.ChestConfig(manifest, config, false);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GenerateConfig();
    }

    private void GenerateConfig()
    {
        var knownChests = this.ChestData
                              .Select(chest =>
                              {
                                  var (key, chestData) = chest;
                                  var name = (from info in this.Craftables where info[0] == key select info[8]).FirstOrDefault() ?? key;
                                  return new KeyValuePair<string, ChestData>(name, chestData);
                              })
                              .OrderBy(chest => chest.Key).ToList();

        // Register mod configuration
        this.GMCM.Register(
            this.Manifest,
            () =>
            {
                this.Config.Reset();
                foreach (var (_, data) in knownChests)
                {
                    ((IChestData)new ChestData()).CopyTo(data);
                }
            },
            () =>
            {
                this.Config.Save();
                this.Helper.Data.WriteJsonFile("assets/chests.json", this.ChestData);
            });

        // General
        this.GeneralConfig();

        // Pages
        this.GMCM.API.AddPageLink(this.Manifest, "Features", I18n.Section_Features_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Features_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Controls", I18n.Section_Controls_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Controls_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Chests", I18n.Section_Chests_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Chests_Description);

        // Features
        this.GMCM.API.AddPage(this.Manifest, "Features");
        this.ChestConfig(this.Manifest, this.Config.DefaultChest, true);

        // Controller
        this.GMCM.API.AddPage(this.Manifest, "Controls");
        this.ControlsConfig(this.Config.ControlScheme);

        // Chests
        this.GMCM.API.AddPage(this.Manifest, "Chests");

        foreach (var (name, _) in knownChests)
        {
            this.GMCM.API.AddPageLink(
                this.Manifest,
                name,
                () => name);
        }

        foreach (var (name, data) in knownChests)
        {
            this.GMCM.API.AddPage(this.Manifest, name);
            this.ChestConfig(this.Manifest, data);
        }
    }

    private void GeneralConfig()
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Categorize Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CategorizeChest,
            value => this.Config.CategorizeChest = value,
            I18n.Config_CategorizeChest_Name,
            I18n.Config_CategorizeChest_Tooltip,
            nameof(CategorizeChest));

        // Slot Lock
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.SlotLock,
            value => this.Config.SlotLock = value,
            I18n.Config_SlotLock_Name,
            I18n.Config_SlotLock_Tooltip,
            nameof(SlotLock));

        // Custom Color Picker Area
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetAreaString(this.Config.CustomColorPickerArea),
            value => this.Config.CustomColorPickerArea = Enum.TryParse(value, out ComponentArea area) ? area : ComponentArea.Right,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            new[] { ComponentArea.Left, ComponentArea.Right }.Select(FormatHelper.GetAreaString).ToArray(),
            FormatHelper.FormatArea,
            nameof(this.Config.CustomColorPickerArea));

        // Search Tag Symbol
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.SearchTagSymbol.ToString(),
            value => this.Config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip,
            fieldId: nameof(this.Config.SearchTagSymbol));
    }

    /// <summary>
    /// Adds GMCM options for chest data.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="config">The chest data to configure.</param>
    /// <param name="defaultConfig">Set to true if configuring the default chest config options.</param>
    private void ChestConfig(IManifest manifest, IChestData config, bool defaultConfig)
    {
        var optionValues = (defaultConfig
                               ? new[] { FeatureOption.Disabled, FeatureOption.Enabled }
                               : new[] { FeatureOption.Disabled, FeatureOption.Default, FeatureOption.Enabled })
                           .Select(FormatHelper.GetOptionString)
                           .ToArray();
        var rangeValues = (defaultConfig
                              ? new[] { FeatureOptionRange.Disabled, FeatureOptionRange.Inventory, FeatureOptionRange.Location, FeatureOptionRange.World }
                              : new[] { FeatureOptionRange.Disabled, FeatureOptionRange.Default, FeatureOptionRange.Inventory, FeatureOptionRange.Location, FeatureOptionRange.World })
                          .Select(FormatHelper.GetRangeString)
                          .ToArray();
        var defaultOption = defaultConfig ? FeatureOption.Enabled : FeatureOption.Default;
        var defaultRange = defaultConfig ? FeatureOptionRange.Location : FeatureOptionRange.Default;

        this.GMCM.API.AddSectionTitle(manifest, I18n.Section_Features_Name, I18n.Section_Features_Description);

        // Carry Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.CarryChest),
            value => config.CarryChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CarryChest_Name,
            I18n.Config_CarryChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CarryChest));

        // Chest Menu Tabs
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.ChestMenuTabs),
            value => config.ChestMenuTabs = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_ChestMenuTabs_Name,
            I18n.Config_ChestMenuTabs_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(ChestMenuTabs));

        // Collect Items
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.CollectItems),
            value => config.CollectItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CollectItems));

        // Craft from Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetRangeString(config.CraftFromChest),
            value => config.CraftFromChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_CraftFromChest_Name,
            I18n.Config_CraftFromChest_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(CraftFromChest));

        // Craft from Chest Distance
        this.GMCM.API.AddNumberOption(
            manifest,
            () => config.CraftFromChestDistance switch
            {
                -1 => 6,
                _ => config.CraftFromChestDistance,
            },
            value => config.CraftFromChestDistance = value switch
            {
                6 => -1,
                _ => value,
            },
            I18n.Config_CraftFromChestDistance_Name,
            I18n.Config_CraftFromChestDistance_Tooltip,
            defaultConfig ? 1 : 0,
            6,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IChestData.CraftFromChestDistance));

        // Custom Color Picker
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.CustomColorPicker),
            value => config.CustomColorPicker = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CustomColorPicker_Name,
            I18n.Config_CustomColorPicker_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CustomColorPicker));

        // Filter Items
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.FilterItems),
            value => config.FilterItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_FilterItems_Name,
            I18n.Config_FilterItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(FilterItems));

        // Open Held Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.OpenHeldChest),
            value => config.OpenHeldChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(OpenHeldChest));

        // Resize Chest Capacity
        this.GMCM.API.AddNumberOption(
            manifest,
            () => config.ResizeChestCapacity switch
            {
                0 when config.ResizeChest is FeatureOption.Disabled => 0,
                0 => 1, // Default
                -1 => 8, // Unlimited
                _ => 1 + (config.ResizeChestCapacity / 12),
            },
            value =>
            {
                config.ResizeChestCapacity = value switch
                {
                    0 or 1 => 0, // Disabled or Default
                    8 => -1, // Unlimited
                    _ => (value - 1) * 12,
                };
                config.ResizeChest = value switch
                {
                    0 => FeatureOption.Disabled,
                    1 => FeatureOption.Default,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestCapacity_Name,
            I18n.Config_ResizeChestCapacity_Tooltip,
            defaultConfig ? 1 : 0,
            8,
            1,
            FormatHelper.FormatChestCapacity,
            nameof(ResizeChest));

        // Resize Chest Menu
        this.GMCM.API.AddNumberOption(
            manifest,
            () => config.ResizeChestMenuRows switch
            {
                0 when config.ResizeChestMenu is FeatureOption.Disabled => 0,
                0 => 1, // Default
                _ => config.ResizeChestMenuRows + 1,
            },
            value =>
            {
                config.ResizeChestMenuRows = value switch
                {
                    0 or 1 => 0, // Disabled or Default
                    _ => value - 1,
                };
                config.ResizeChestMenu = value switch
                {
                    0 => FeatureOption.Disabled,
                    1 => FeatureOption.Default,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestMenuRows_Name,
            I18n.Config_ResizeChestMenuRows_Tooltip,
            defaultConfig ? 1 : 0,
            7,
            1,
            FormatHelper.FormatChestMenuRows,
            nameof(ResizeChestMenu));

        // Search Items
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.SearchItems),
            value => config.SearchItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(SearchItems));

        // Stash to Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetRangeString(config.StashToChest),
            value => config.StashToChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_StashToChest_Name,
            I18n.Config_StashToChest_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(StashToChest));

        // Stash to Chest Distance
        this.GMCM.API.AddNumberOption(
            manifest,
            () => config.StashToChestDistance switch
            {
                -1 => 6,
                _ => config.StashToChestDistance,
            },
            value => config.StashToChestDistance = value switch
            {
                6 => -1,
                _ => value,
            },
            I18n.Config_StashToChestDistance_Name,
            I18n.Config_StashToChestDistance_Tooltip,
            defaultConfig ? 1 : 0,
            6,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IChestData.StashToChestDistance));

        // Stash to Chest Stacks
        this.GMCM.API.AddBoolOption(
            manifest,
            () => config.StashToChestStacks,
            value => config.StashToChestStacks = value,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip,
            nameof(IChestData.StashToChestStacks));

        // Unload Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(config.UnloadChest),
            value => config.UnloadChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(UnloadChest));
    }

    private void ControlsConfig(IControlScheme config)
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Lock Slot
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.LockSlot,
            value => config.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip,
            nameof(IControlScheme.LockSlot));

        // Open Crafting
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.OpenCrafting,
            value => config.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip,
            nameof(IControlScheme.OpenCrafting));

        // Stash Items
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.StashItems,
            value => config.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip,
            nameof(IControlScheme.StashItems));

        // Scroll Up
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.ScrollUp,
            value => config.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip,
            nameof(IControlScheme.ScrollUp));

        // Scroll Down
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.ScrollDown,
            value => config.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip,
            nameof(IControlScheme.ScrollDown));

        // Previous Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.PreviousTab,
            value => config.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip,
            nameof(IControlScheme.PreviousTab));

        // Next Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.NextTab,
            value => config.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip,
            nameof(IControlScheme.NextTab));
    }
}