﻿using ColossalFramework.UI;
using NodeMarkup.Manager;
using NodeMarkup.Tools;
using NodeMarkup.UI.Panel;
using NodeMarkup.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NodeMarkup.UI.Editors
{
    public abstract class Editor : UIPanel
    {
        public static Dictionary<Style.StyleType, string> SpriteNames { get; set; }
        public static UITextureAtlas StylesAtlas { get; } = GetStylesIcons();
        private static UITextureAtlas GetStylesIcons()
        {
            SpriteNames = new Dictionary<Style.StyleType, string>()
            {
                {Style.StyleType.LineSolid, nameof(Style.StyleType.LineSolid) },
                {Style.StyleType.LineDashed,  nameof(Style.StyleType.LineDashed) },
                {Style.StyleType.LineDoubleSolid,   nameof(Style.StyleType.LineDoubleSolid) },
                {Style.StyleType.LineDoubleDashed, nameof(Style.StyleType.LineDoubleDashed) },
                {Style.StyleType.LineSolidAndDashed, nameof(Style.StyleType.LineSolidAndDashed) },
                {Style.StyleType.LineSharkTeeth, nameof(Style.StyleType.LineSharkTeeth) },
                {Style.StyleType.StopLineSolid, nameof(Style.StyleType.StopLineSolid) },
                {Style.StyleType.StopLineDashed, nameof(Style.StyleType.StopLineDashed) },
                {Style.StyleType.StopLineDoubleSolid, nameof(Style.StyleType.StopLineDoubleSolid) },
                {Style.StyleType.StopLineDoubleDashed, nameof(Style.StyleType.StopLineDoubleDashed) },
                {Style.StyleType.StopLineSolidAndDashed, nameof(Style.StyleType.StopLineSolidAndDashed) },
                {Style.StyleType.StopLineSharkTeeth, nameof(Style.StyleType.StopLineSharkTeeth) },
                {Style.StyleType.FillerStripe, nameof(Style.StyleType.FillerStripe) },
                {Style.StyleType.FillerGrid, nameof(Style.StyleType.FillerGrid) },
                {Style.StyleType.FillerSolid, nameof(Style.StyleType.FillerSolid) },
                {Style.StyleType.FillerChevron, nameof(Style.StyleType.FillerChevron) },
                {Style.StyleType.CrosswalkExistent, nameof(Style.StyleType.CrosswalkExistent) },
                {Style.StyleType.CrosswalkZebra, nameof(Style.StyleType.CrosswalkZebra) },
                {Style.StyleType.CrosswalkDoubleZebra, nameof(Style.StyleType.CrosswalkDoubleZebra) },
                {Style.StyleType.CrosswalkParallelSolidLines, nameof(Style.StyleType.CrosswalkParallelSolidLines) },
                {Style.StyleType.CrosswalkParallelDashedLines, nameof(Style.StyleType.CrosswalkParallelDashedLines) },
                {Style.StyleType.CrosswalkLadder, nameof(Style.StyleType.CrosswalkLadder) },
                {Style.StyleType.CrosswalkSolid, nameof(Style.StyleType.CrosswalkSolid) },
                {Style.StyleType.CrosswalkChessBoard, nameof(Style.StyleType.CrosswalkChessBoard) },
            };

            var atlas = TextureUtil.GetAtlas(nameof(StylesAtlas));
            if (atlas == UIView.GetAView().defaultAtlas)
            {
                atlas = TextureUtil.CreateTextureAtlas("Styles.png", nameof(StylesAtlas), 19, 19, SpriteNames.Values.ToArray());
            }

            return atlas;
        }
        protected NodeMarkupTool Tool => NodeMarkupTool.Instance;
        public NodeMarkupPanel NodeMarkupPanel { get; private set; }
        protected Markup Markup => NodeMarkupPanel.Markup;

        protected UIScrollablePanel ItemsPanel { get; set; }
        protected UIScrollablePanel ContentPanel { get; set; }

        protected UILabel EmptyLabel { get; set; }

        public abstract string Name { get; }
        public abstract string EmptyMessage { get; }

        public Editor()
        {
            clipChildren = true;
            atlas = TextureUtil.InGameAtlas;
            backgroundSprite = "UnlockingItemBackground";

            AddItemsPanel();
            AddSettingPanel();
            AddEmptyLabel();
        }
        private UIScrollablePanel AddPanel(string background)
        {
            var panel = AddUIComponent<UIScrollablePanel>();

            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            panel.scrollWheelDirection = UIOrientation.Vertical;
            panel.builtinKeyNavigation = true;
            panel.clipChildren = true;

            panel.atlas = TextureUtil.InGameAtlas;
            panel.backgroundSprite = background;

            this.AddScrollbar(panel);

            return panel;
        }

        private void AddItemsPanel()
        {
            ItemsPanel = AddPanel("ScrollbarTrack");
            ItemsPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            ItemsPanel.eventSizeChanged += ItemsPanelSizeChanged;
            ItemsPanel.verticalScrollbar.eventVisibilityChanged += ItemsScrollbarVisibilityChanged;
        }

        private void ItemsPanelSizeChanged(UIComponent component, Vector2 value)
        {
            foreach (var item in ItemsPanel.components)
                item.width = ItemsPanel.width;
        }
        private void ItemsScrollbarVisibilityChanged(UIComponent component, bool value)
        {
            ItemsPanel.width = size.x / 10 * 3 - (ItemsPanel.verticalScrollbar.isVisible ? ItemsPanel.verticalScrollbar.width : 0);
        }

        private void AddSettingPanel()
        {
            ContentPanel = AddPanel("UnlockingItemBackground");
            ContentPanel.autoLayoutPadding = new RectOffset(10, 10, 0, 0);
            ContentPanel.eventSizeChanged += SettingsPanelSizeChanged;
            ContentPanel.verticalScrollbar.eventVisibilityChanged += SettingsScrollbarVisibilityChanged;
        }

        private void SettingsPanelSizeChanged(UIComponent component, Vector2 value)
        {
            foreach (var item in ContentPanel.components)
                item.width = ContentPanel.width - ContentPanel.autoLayoutPadding.horizontal;
        }
        private bool InProgress { get; set; } = false;
        private void SettingsScrollbarVisibilityChanged(UIComponent component, bool value)
        {
            if (InProgress && !value)
                return;

            InProgress = true;
            ContentPanel.width = size.x / 10 * 7 - (value ? ContentPanel.verticalScrollbar.width : 0);
            InProgress = false;
        }
        private void AddEmptyLabel()
        {
            EmptyLabel = AddUIComponent<UILabel>();
            EmptyLabel.textAlignment = UIHorizontalAlignment.Center;
            EmptyLabel.verticalAlignment = UIVerticalAlignment.Middle;
            EmptyLabel.padding = new RectOffset(10, 10, 0, 0);
            EmptyLabel.wordWrap = true;
            EmptyLabel.autoSize = false;

            SwitchEmpty();
        }
        protected void SwitchEmpty()
        {
            if (ItemsPanel.components.Any())
                EmptyLabel.isVisible = false;
            else
            {
                EmptyLabel.isVisible = true;
                EmptyLabel.text = EmptyMessage;
            }
        }

        public virtual void Init(NodeMarkupPanel panel)
        {
            NodeMarkupPanel = panel;
        }
        public virtual void UpdateEditor() { }
        protected virtual void ClearItems() { }
        protected virtual void ClearSettings() { }
        protected virtual void FillItems() { }
        public virtual void Select(int index) { }
        public virtual void Render(RenderManager.CameraInfo cameraInfo) { }
        public virtual bool OnShortcut(Event e) => false;

        protected abstract void ItemClick(UIComponent component, UIMouseEventParameter eventParam);
        protected abstract void ItemHover(UIComponent component, UIMouseEventParameter eventParam);
        protected abstract void ItemLeave(UIComponent component, UIMouseEventParameter eventParam);

        public void StopScroll() => ContentPanel.scrollWheelDirection = UIOrientation.Horizontal;
        public void StartScroll() => ContentPanel.scrollWheelDirection = UIOrientation.Vertical;
    }
    public abstract class Editor<EditableItemType, EditableObject, ItemIcon> : Editor
        where EditableItemType : EditableItem<EditableObject, ItemIcon>
        where ItemIcon : UIComponent
        where EditableObject : class, IDeletable
    {
        protected PropertyGroupPanel GroupPanel { get; private set; }
        protected virtual bool UseGroupPanel => false;
        protected UIComponent PropertiesPanel => UseGroupPanel ? (UIComponent)GroupPanel : (UIComponent)ContentPanel;

        EditableItemType _selectItem;

        protected EditableItemType HoverItem { get; set; }
        protected bool IsHoverItem => HoverItem != null;
        protected EditableItemType SelectItem
        {
            get => _selectItem;
            private set
            {
                if (_selectItem != null)
                    _selectItem.IsSelect = false;

                _selectItem = value;

                if (_selectItem != null)
                    _selectItem.IsSelect = true;
            }
        }
        public EditableObject EditObject => SelectItem?.Object;

        public Editor()
        {
            if (UseGroupPanel)
                ContentPanel.autoLayoutPadding = new RectOffset(10, 10, 10, 10);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            var itemsPanelWidth = size.x / 10 * 3 - (ItemsPanel.verticalScrollbar.isVisible ? ItemsPanel.verticalScrollbar.width : 0);
            ItemsPanel.size = new Vector2(itemsPanelWidth, size.y);
            ItemsPanel.relativePosition = new Vector2(0, 0);

            var settingsPanelWidth = size.x / 10 * 7 - (ContentPanel.verticalScrollbar.isVisible ? ContentPanel.verticalScrollbar.width : 0);
            ContentPanel.size = new Vector2(settingsPanelWidth, size.y);
            ContentPanel.relativePosition = new Vector2(size.x / 10 * 3, 0);

            EmptyLabel.size = new Vector2(size.x / 10 * 7, size.y / 2);
            EmptyLabel.relativePosition = ContentPanel.relativePosition;
        }
        public virtual EditableItemType AddItem(EditableObject editableObject)
        {
            var item = GetItem(ItemsPanel);
            InitItem(item, editableObject);

            SwitchEmpty();

            return item;
        }
        protected virtual EditableItemType GetItem(UIComponent parent)
        {
            var newItem = ComponentPool.Get<EditableItemType>(parent);
            newItem.width = parent.width;
            return newItem;
        }
        protected void InitItem(EditableItemType item, EditableObject editableObject)
        {
            item.Init(editableObject);
            item.eventClick += ItemClick;
            item.eventMouseEnter += ItemHover;
            item.eventMouseLeave += ItemLeave;
            item.OnDelete += ItemDelete;
        }

        protected void ItemDelete(EditableItem<EditableObject, ItemIcon> deleteItem)
        {
            if (!(deleteItem is EditableItemType item))
                return;

            Tool.DeleteItem(item.Object, Delete);

            void Delete()
            {
                OnObjectDelete(item.Object);
                var isSelect = item == SelectItem;
                DeleteItem(item);
                if (isSelect)
                {
                    ClearSettings();
                    Select(0);
                }
            }
        }

        protected override void ClearItems() => ClearItems(ItemsPanel);
        protected void ClearItems(UIComponent parent)
        {
            _selectItem = null;

            var components = parent.components.ToArray();
            foreach (var component in components)
            {
                if (component is EditableItemType item)
                    DeleteItem(item);
                else
                    DeleteUIComponent(component);
            }
        }
        protected virtual void DeleteItem(EditableItemType item)
        {
            if (HoverItem == item)
                HoverItem = null;

            item.eventClick -= ItemClick;
            item.eventMouseEnter -= ItemHover;
            item.eventMouseLeave -= ItemLeave;
            item.OnDelete -= ItemDelete;
            ComponentPool.Free(item);

            SwitchEmpty();
        }
        protected void DeleteUIComponent(UIComponent component)
        {
            ItemsPanel.RemoveUIComponent(component);
            Destroy(component);
        }
        protected virtual EditableItemType GetItem(EditableObject editObject) => ItemsPanel.components.OfType<EditableItemType>().FirstOrDefault(c => ReferenceEquals(c.Object, editObject));
        public virtual void UpdateEditor(EditableObject selectObject = null)
        {
            var editObject = EditObject;

            if (selectObject != null && selectObject == editObject)
            {
                OnObjectUpdate();
                return;
            }

            ClearItems();
            if (Markup != null)
                FillItems();

            if (selectObject != null && GetItem(selectObject) is EditableItemType selectItem)
                Select(selectItem);
            else if (editObject != null && GetItem(editObject) is EditableItemType editItem)
            {
                SelectItem = editItem;
                ScrollTo(SelectItem);
                OnObjectUpdate();
            }
            else
            {
                SelectItem = null;
                ClearSettings();
                Select(0);
            }

            SwitchEmpty();
        }
        public override void UpdateEditor() => UpdateEditor(null);

        protected override void ClearSettings()
        {
            if (UseGroupPanel)
                ClearSettings(GroupPanel);

            ClearSettings(ContentPanel);

            OnClear();
        }
        private void ClearSettings(UIComponent parent)
        {
            if (parent == null)
                return;

            var components = parent.components.ToArray();
            foreach (var component in components)
            {
                if (component != EmptyLabel)
                    ComponentPool.Free(component);
            }
        }

        protected override void ItemClick(UIComponent component, UIMouseEventParameter eventParam) => ItemClick((EditableItemType)component);
        protected virtual void ItemClick(EditableItemType item)
        {
            ContentPanel.autoLayout = false;
            ClearSettings();
            SelectItem = item;
            if (UseGroupPanel)
                GroupPanel = ComponentPool.Get<PropertyGroupPanel>(ContentPanel);
            OnObjectSelect();
            if (UseGroupPanel)
                GroupPanel.Init();
            ContentPanel.autoLayout = true;
        }
        protected override void ItemHover(UIComponent component, UIMouseEventParameter eventParam) => HoverItem = component as EditableItemType;
        protected override void ItemLeave(UIComponent component, UIMouseEventParameter eventParam) => HoverItem = null;
        protected virtual void OnObjectSelect() { }
        protected virtual void OnClear() { }
        protected virtual void OnObjectDelete(EditableObject editableObject) { }
        protected virtual void OnObjectUpdate() { }
        public override void Select(int index)
        {
            if (ItemsPanel.components.Count > index && ItemsPanel.components[index] is EditableItemType item)
                Select(item);
        }
        public virtual void Select(EditableItemType item)
        {
            item.SimulateClick();
            item.Focus();
            ScrollTo(item);
        }
        public virtual void ScrollTo(EditableItemType item)
        {
            ItemsPanel.ScrollToBottom();
            ItemsPanel.ScrollIntoView(item);
        }
        protected virtual void RefreshItems()
        {
            foreach (var item in ItemsPanel.components.OfType<EditableItemType>())
                item.Refresh();
        }
    }
}
