﻿using ModsCommon;
using ModsCommon.Utilities;
using NodeMarkup.Manager;
using NodeMarkup.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static ColossalFramework.Math.VectorUtils;

namespace NodeMarkup.Tools
{
    public abstract class BaseMakeItemToolMode : NodeMarkupToolMode
    {
        protected List<MarkupPoint> TargetPoints { get; set; } = new List<MarkupPoint>();

        public MarkupPoint HoverPoint { get; protected set; } = null;
        public MarkupPoint SelectPoint { get; protected set; } = null;

        protected bool IsHoverPoint => HoverPoint != null;
        protected bool IsSelectPoint => SelectPoint != null;

        protected override void Reset(IToolMode prevMode)
        {
            HoverPoint = null;
            SelectPoint = null;
            SetTarget();
        }

        public override void OnToolUpdate()
        {
            if (SingletonTool<NodeMarkupTool>.Instance.MouseRayValid)
            {
                foreach (var point in TargetPoints)
                {
                    if (point.IsHover(SingletonTool<NodeMarkupTool>.Instance.MouseRay))
                    {
                        HoverPoint = point;
                        return;
                    }
                }
            }

            if (IsSelectPoint && SelectPoint.Type == MarkupPoint.PointType.Enter && (SelectPoint.Enter.SupportPoints & MarkupPoint.PointType.Normal) != 0)
            {
                var connectLine = SingletonTool<NodeMarkupTool>.Instance.Ray.GetRayPosition(Markup.Position.y, out _) - SelectPoint.Position;
                if (connectLine.magnitude >= 2 && 135 <= Vector3.Angle(XZ(SelectPoint.Direction), XZ(connectLine)) && SelectPoint.Enter.TryGetPoint(SelectPoint.Num, MarkupPoint.PointType.Normal, out MarkupPoint normalPoint))
                {
                    HoverPoint = normalPoint;
                    return;
                }
            }

            HoverPoint = null;
        }
        public override string GetToolInfo()
        {
            var pointPair = new MarkupPointPair(SelectPoint, HoverPoint);
            var exist = Tool.Markup.ExistLine(pointPair);

            if (pointPair.IsStopLine)
                return exist ? $"{Localize.Tool_InfoDeleteStopLine}\n{string.Format(Localize.Tool_InfoSelectLine, LocalizeExtension.Ctrl.AddInfoColor())}" : Tool.GetModifierToolTip<StopLineStyle.StopLineType>(Localize.Tool_InfoCreateStopLine);
            else if (pointPair.IsCrosswalk)
                return exist ? $"{Localize.Tool_InfoDeleteCrosswalk}\n{string.Format(Localize.Tool_InfoSelectCrosswalk, LocalizeExtension.Ctrl.AddInfoColor())}" : Tool.GetModifierToolTip<CrosswalkStyle.CrosswalkType>(Localize.Tool_InfoCreateCrosswalk);
            else if (pointPair.IsNormal)
                return exist ? $"{Localize.Tool_InfoDeleteNormalLine}\n{string.Format(Localize.Tool_InfoSelectLine, LocalizeExtension.Ctrl.AddInfoColor())}" : Tool.GetModifierToolTip<RegularLineStyle.RegularLineType>(Localize.Tool_InfoCreateNormalLine);
            else
                return exist ? $"{Localize.Tool_InfoDeleteLine}\n{string.Format(Localize.Tool_InfoSelectLine, LocalizeExtension.Ctrl.AddInfoColor())}" : Tool.GetModifierToolTip<RegularLineStyle.RegularLineType>(Localize.Tool_InfoCreateLine);
        }
        public override void OnPrimaryMouseClicked(Event e)
        {
            SelectPoint = HoverPoint;
            SetTarget(SelectPoint);
        }
        public override void OnSecondaryMouseClicked()
        {
            if (IsSelectPoint)
            {
                SelectPoint = null;
                SetTarget();
            }
            else
            {
                Tool.SetMarkup(null);
                Tool.SetMode(ToolModeType.Select);
            }
        }
        public override bool OnEscape()
        {
            if (IsSelectPoint)
            {
                SelectPoint = null;
                SetTarget();
                return true;
            }
            else
                return false;
        }

        #region SET TARGET

        protected void SetTarget(MarkupPoint ignore = null)
        {
            TargetPoints.Clear();
            foreach (var enter in Tool.Markup.Enters)
                TargetPoints.AddRange(GetTarget(enter, ignore));
        }
        protected abstract IEnumerable<MarkupPoint> GetTarget(Enter enter, MarkupPoint ignore);
        protected int Find(int[] allow, int idx, int sign)
        {
            do
                idx += sign;
            while (idx >= 0 && idx < allow.Length && allow[idx] != 2);

            return idx;
        }
        protected void SetNotAllow(int[] allow, int idx, int sign)
        {
            idx += sign;
            while (idx >= 0 && idx < allow.Length)
            {
                allow[idx] = 0;
                idx += sign;
            }
        }

        #endregion

        protected void RenderPointsOverlay(RenderManager.CameraInfo cameraInfo, bool splitPoint = true)
        {
            foreach (var point in TargetPoints)
                point.Render(new OverlayData(cameraInfo) { SplitPoint = splitPoint });
        }
    }
}
