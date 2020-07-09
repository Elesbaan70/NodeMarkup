﻿using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace NodeMarkup.UI
{
    public class UIUtils
    {
        public static UIUtils Instance
        {
            get
            {
                bool flag = instance == null;
                if (flag)
                {
                    instance = new UIUtils();
                }
                return instance;
            }
        }
        private void FindUIRoot()
        {
            uiRoot = null;
            foreach (UIView uiview in UnityEngine.Object.FindObjectsOfType<UIView>())
            {
                bool flag = uiview.transform.parent == null && uiview.name == "UIView";
                if (flag)
                {
                    uiRoot = uiview;
                    break;
                }
            }
        }
        public string GetTransformPath(Transform transform)
        {
            string text = transform.name;
            Transform parent = transform.parent;
            while (parent != null)
            {
                text = parent.name + "/" + text;
                parent = parent.parent;
            }
            return text;
        }
        public T FindComponent<T>(string name, UIComponent parent = null, FindOptions options = FindOptions.None) where T : UIComponent
        {
            bool flag = uiRoot == null;
            if (flag)
            {
                FindUIRoot();
                bool flag2 = uiRoot == null;
                if (flag2)
                {
                    return default;
                }
            }
            foreach (T t in UnityEngine.Object.FindObjectsOfType<T>())
            {
                bool flag3 = (options & FindOptions.NameContains) > FindOptions.None;
                bool flag4;
                if (flag3)
                {
                    flag4 = t.name.Contains(name);
                }
                else
                {
                    flag4 = (t.name == name);
                }
                bool flag5 = !flag4;
                if (!flag5)
                {
                    bool flag6 = parent != null;
                    Transform transform;
                    if (flag6)
                    {
                        transform = parent.transform;
                    }
                    else
                    {
                        transform = uiRoot.transform;
                    }
                    Transform parent2 = t.transform.parent;
                    while (parent2 != null && parent2 != transform)
                    {
                        parent2 = parent2.parent;
                    }
                    bool flag7 = parent2 == null;
                    if (!flag7)
                    {
                        return t;
                    }
                }
            }
            return default;
        }

        public static IEnumerable<T> GetCompenentsWithName<T>(string name) where T : UIComponent
        {
            T[] components = GameObject.FindObjectsOfType<T>();
            foreach (T component in components)
            {
                if (component.name == name)
                    yield return component;
            }
        }
        private static UIUtils instance = null;
        private UIView uiRoot = null;
        [Flags]
        public enum FindOptions
        {
            None = 0,
            NameContains = 1
        }
    }
}
