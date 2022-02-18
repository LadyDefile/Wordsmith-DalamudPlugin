﻿using System;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Logging;
using Dalamud.Interface.Windowing;
using Wordsmith.Gui;
using ImGuiNET;
using Dalamud.Interface;

namespace Wordsmith
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public static class WordsmithUI
    {
        private static List<Window> _windows = new();
        public static Window[] Windows => _windows.ToArray();

        public static readonly WindowSystem WindowSystem = new WindowSystem("Wordsmith");

        // passing in the image here just for simplicity
        public static void ShowThesaurus() => Show<ThesaurusUI>($"{Wordsmith.AppName} - Thesaurus");
        public static void ShowScratchPad(int id) => Show<ScratchPadUI>($"{Wordsmith.AppName} - Scratch Pad #{id}");
        public static void ShowScratchPad(string tellTarget) => _windows.Add(new ScratchPadUI(tellTarget));
        public static void ShowScratchPadHelp() => Show<ScratchPadHelpUI>($"{Wordsmith.AppName} - Scratch Pad Help");
        public static void ShowSettings() => Show<SettingsUI>($"{Wordsmith.AppName} - Settings");
        public static void ShowRestoreSettings() => Show<RestoreDefaultsUI>($"{Wordsmith.AppName} - Restore Default Settings");
        public static void ShowResetDictionary() => Show<ResetDictionaryUI>($"{Wordsmith.AppName} - Reset Dictionary");

        private static void Show<T>(string name)
        {
            // If the given type is not a subclass of Window leave the method
            if (!typeof(T).IsSubclassOf(typeof(Window)))
                return;
            
            // Attempt to get the window by name.
            Window? w = _windows.FirstOrDefault(w => w.WindowName == name);

            // If the result is null, create a new window
            if (w == null)
                #pragma warning disable CS8604 // Possible null reference argument.
                _windows.Add(Activator.CreateInstance(typeof(T)) as Window);
                #pragma warning restore CS8604 // Possible null reference argument.

                // If the result wasn't null, open the window
            else
                w.IsOpen = true;
            
        }
        public static void RemoveWindow(Window w)
        {
            _windows.Remove(w);
            WindowSystem.RemoveWindow(w);
        }

        public static void Draw()
        {
            try { WordsmithUI.WindowSystem.Draw(); }
            catch (InvalidOperationException e)
            {
                // If the message isn't about collection being modified, log it. Otherwise
                // Discard the error.
                if (!e.Message.StartsWith("Collection was modified"))
                    PluginLog.LogError($"{e.Message}");
            }
            catch (Exception e) { PluginLog.LogError($"{e} :: {e.Message}"); }
        }
    }
}
