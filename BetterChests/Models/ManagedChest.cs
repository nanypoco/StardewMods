﻿namespace BetterChests.Models;

using System.Linq;
using Common.Helpers.ItemMatcher;
using StardewValley;
using StardewValley.Objects;

internal record ManagedChest
{
    public ManagedChest(Chest chest, ChestType config)
    {
        this.Chest = chest;
        this.Config = config;
        this.ItemMatcher = new(true);

        if (this.Chest.modData.TryGetValue("FilterItems", out var filterItems) && !string.IsNullOrWhiteSpace(filterItems))
        {
            this.ItemMatcher.StringValue = filterItems;
        }
    }

    public Chest Chest { get; }

    public ChestType Config { get; }

    public ItemMatcher ItemMatcher { get; }

    public bool AcceptsItem(Item item)
    {
        return this.Config.ItemMatcher.Matches(item) && this.ItemMatcher.Matches(item);
    }

    public Item StashItem(Item item, bool existingStacks = false)
    {
        var stack = item.Stack;

        if (this.AcceptsItem(item))
        {
            var tmp = this.Chest.addItem(item);
            if (tmp is null || tmp.Stack <= 0)
            {
                return null;
            }

            if (tmp.Stack != stack)
            {
                item.Stack = tmp.Stack;
            }
        }

        if (existingStacks)
        {
            foreach (var chestItem in this.Chest.items.Where(chestItem => chestItem.canStackWith(item)))
            {
                if (chestItem.getRemainingStackSpace() > 0)
                {
                    item.Stack = chestItem.addToStack(item);
                }

                if (item.Stack <= 0)
                {
                    return null;
                }
            }
        }

        return item;
    }
}