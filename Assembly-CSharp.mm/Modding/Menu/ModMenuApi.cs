using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modding
{

    public class MenuEntry
    {
        public string Name;
        public string Description;
        public string[] Values;

        public Action<int> Saver;

        public Func<int> Loader;
    }


    public struct ModToggleDelegates
    {
        public Action<bool> SetModEnabled;
        public Func<bool> GetModEnabled;
    }





    public interface IMenuMod : IMod
    {

        bool ToggleButtonInsideMenu { get; }

        List<MenuEntry> GetMenuData(MenuEntry toggleButtonEntry);
    }


    public interface ICustomMenuMod : IMod
    {
        bool ToggleButtonInsideMenu { get; }


        MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates);
    }


    public interface IMenuButtonText
    {
        string GetMenuButtonText();
    }


    public abstract class ModMenuRow
    {
        public string Label;
        public string Description;

        public static ModMenuRow Option(string label, string[] values, Func<int> getter, Action<int> setter, string description = null) =>
            new ModMenuOptionRow { Label = label, Values = values, Getter = getter, Setter = setter, Description = description };

        public static ModMenuRow Button(string label, Action onSubmit, string description = null) =>
            new ModMenuButtonRow { Label = label, OnSubmit = onSubmit, Description = description };

        public static ModMenuRow FromEntry(MenuEntry e) =>
            Option(e.Name, e.Values, e.Loader, e.Saver, e.Description);
    }

    public sealed class ModMenuOptionRow : ModMenuRow
    {
        public string[] Values;
        public Func<int> Getter;
        public Action<int> Setter;
    }

    public sealed class ModMenuButtonRow : ModMenuRow
    {
        public Action OnSubmit;
    }
}
