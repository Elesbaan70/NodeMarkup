﻿using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using ICities;
using NodeMarkup.Manager;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NodeMarkup.UI
{
    public static class Settings
    {
        public static string SettingsFile => $"{nameof(NodeMarkup)}{nameof(SettingsFile)}";

        public static SavedFloat RenderDistance { get; } = new SavedFloat(nameof(RenderDistance), SettingsFile, 300f, true);
        public static SavedBool ShowToolTip { get; } = new SavedBool(nameof(ShowToolTip), SettingsFile, true, true);
        public static SavedBool DeleteWarnings { get; } = new SavedBool(nameof(DeleteWarnings), SettingsFile, true, true);
        public static SavedBool QuickRuleSetip { get; } = new SavedBool(nameof(QuickRuleSetip), SettingsFile, false, true);
        public static SavedString Templates { get; } = new SavedString(nameof(Templates), SettingsFile, string.Empty, true);

        static Settings()
        {
            if (GameSettings.FindSettingsFileByName(SettingsFile) == null)
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SettingsFile } });
        }

        public static void OnSettingsUI(UIHelperBase helper)
        {
            AddKeyMapping(helper);
            AddGeneral(helper);
            AddOther(helper);
        }
        private static void AddKeyMapping(UIHelperBase helper)
        {
            UIHelper group = helper.AddGroup(Localize.Settings_Shortcuts) as UIHelper;
            UIPanel panel = group.self as UIPanel;

            var keymappings = panel.gameObject.AddComponent<KeymappingsPanel>();
            keymappings.AddKeymapping(Localize.Settings_ActivateTool, NodeMarkupTool.ActivationShortcut);
            keymappings.AddKeymapping(Localize.Settings_DeleteAllNodeLines, NodeMarkupTool.DeleteAllShortcut);
            keymappings.AddKeymapping(Localize.Settings_AddNewLineRule, NodeMarkupTool.AddRuleShortcut);
        }
        private static void AddGeneral(UIHelperBase helper)
        {
            UIHelper group = helper.AddGroup(Localize.Settings_General) as UIHelper;

            AddDistanceSetting(group);
            AddShowToolTipsSetting(group);
            AddDeleteRequest(group);
            AddQuickRuleSetup(group);
        }
        private static void AddDistanceSetting(UIHelper group)
        {
            UITextField distanceField = null;
            distanceField = group.AddTextfield(Localize.Settings_RenderDistance, RenderDistance.ToString(), OnDistanceChanged, OnDistanceSubmitted) as UITextField;

            void OnDistanceChanged(string distance) { }
            void OnDistanceSubmitted(string text)
            {
                if (float.TryParse(text, out float distance))
                {
                    if (distance < 0)
                        distance = 300;

                    RenderDistance.value = distance;
                    distanceField.text = distance.ToString();
                }
                else
                {
                    distanceField.text = RenderDistance.ToString();
                }
            }
        }
        private static void AddShowToolTipsSetting(UIHelper group)
        {
            var showCheckBox = group.AddCheckbox(Localize.Settings_ShowTooltips, ShowToolTip, OnShowToolTipsChanged) as UICheckBox;

            void OnShowToolTipsChanged(bool show) => ShowToolTip.value = show;
        }
        private static void AddDeleteRequest(UIHelper group)
        {
            var requestCheckBox = group.AddCheckbox(Localize.Settings_ShowDeleteWarnings, DeleteWarnings, OnDeleteRequestChanged) as UICheckBox;

            void OnDeleteRequestChanged(bool request) => DeleteWarnings.value = request;
        }
        private static void AddQuickRuleSetup(UIHelper group)
        {
            var quickRuleSetupCheckBox = group.AddCheckbox(Localize.Settings_QuickRuleSetup, QuickRuleSetip, OnQuickRuleSetuptChanged) as UICheckBox;

            void OnQuickRuleSetuptChanged(bool request) => QuickRuleSetip.value = request;
        }
        private static void AddOther(UIHelperBase helper)
        {
            if (SceneManager.GetActiveScene().name is string scene && (scene == "MainMenu" || scene == "IntroScreen"))
                return;

            UIHelper group = helper.AddGroup(Localize.Settings_Other) as UIHelper;

            AddDeleteAll(group);
            AddDump(group);
            AddImport(group);
        }
        private static void AddDeleteAll(UIHelper group)
        {
            var button = group.AddButton(Localize.Settings_DeleteMarkingButton, Click) as UIButton;
            button.textColor = Color.red;

            void Click()
            {
                var messageBox = MessageBox.ShowModal<YesNoMessageBox>();
                messageBox.CaprionText = Localize.Settings_DeleteMarkingCaption;
                messageBox.MessageText = Localize.Settings_DeleteMarkingMessage;
                messageBox.OnButton1Click = Сonfirmed;
            }
            bool Сonfirmed()
            {
                MarkupManager.DeleteAll();
                return true;
            }
        }
        private static void AddDump(UIHelper group)
        {
            var button = group.AddButton(Localize.Settings_DumpMarkingButton, Click) as UIButton;

            void Click()
            {
                var result = Serializer.OnDumpData(out string path);

                if (result)
                {
                    var messageBox = MessageBox.ShowModal<TwoButtonMessageBox>();
                    messageBox.CaprionText = Localize.Settings_DumpMarkingCaption;
                    messageBox.MessageText = Localize.Settings_DumpMarkingMessageSuccess;
                    messageBox.Button1Text = Localize.Settings_DumpMarkingButton1;
                    messageBox.Button2Text = Localize.Settings_DumpMarkingButton2;
                    messageBox.OnButton1Click = CopyToClipboard;

                    bool CopyToClipboard()
                    {
                        Clipboard.text = path;
                        return false;
                    }
                }
                else
                {
                    var messageBox = MessageBox.ShowModal<OkMessageBox>();
                    messageBox.CaprionText = Localize.Settings_DumpMarkingCaption;
                    messageBox.MessageText = Localize.Settings_DumpMarkingMessageFailed;
                }
            }
        }
        private static void AddImport(UIHelper group)
        {
            var button = group.AddButton(Localize.Settings_ImportMarkingButton, Click) as UIButton;

            void Click()
            {
                var messageBox = MessageBox.ShowModal<YesNoMessageBox>();
                messageBox.CaprionText = Localize.Settings_ImportMarkingCaption;
                messageBox.MessageText = Localize.Settings_ImportMarkingMessage;
                messageBox.OnButton1Click = StartImport;

            }
            bool StartImport()
            {
                var result = Serializer.OnImportData();

                var resultMessageBox = MessageBox.ShowModal<OkMessageBox>();
                resultMessageBox.CaprionText = Localize.Settings_ImportMarkingCaption;
                resultMessageBox.MessageText = result ? Localize.Settings_ImportMarkingMessageSuccess : Localize.Settings_ImportMarkingMessageFailed;

                return true;
            }
        }
    }
}
