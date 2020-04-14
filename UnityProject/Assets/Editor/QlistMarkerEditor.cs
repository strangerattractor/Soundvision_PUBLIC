using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using UnityEngine.Playables;

namespace cylvester
{

    [CustomTimelineEditor(typeof(QlistMarker))]
    public class QlistMarkerEditor : MarkerEditor
    {
        static GUIContent s_Temp = new GUIContent();
     
        // public override MarkerDrawOptions GetMarkerOptions(IMarker marker)
        // {
        //     var snapPoint = (QlistMarker) marker;
     
        //     var options = base.GetMarkerOptions(marker);
        //     options.tooltip = snapPoint.tooltip;
        //     return options;
        // }
     
     
        public override void OnCreate(IMarker marker, IMarker clonedFrom)
        {
            var snapPoint = (QlistMarker) marker;
            // var track = marker.parent;
            // var lastMarker = track.GetMarkers().OfType<QlistMarker>().LastOrDefault(x => x != snapPoint);
            // if (lastMarker != null)
            // {
            //     float h, s, v;
            //     Color.RGBToHSV(lastMarker.snapLineColor, out h, out s, out v);
            //     h = (h + 0.2f) % 1.0f;
            //     var c = Color.HSVToRGB(h, s, v);
            //     snapPoint.snapLineColor.r = c.r;
            //     snapPoint.snapLineColor.g = c.g;
            //     snapPoint.snapLineColor.b = c.b;
            // }
        }
     
        public override void DrawOverlay(IMarker marker, MarkerUIStates uiState, MarkerOverlayRegion region)
        {
            var snapPoint = (QlistMarker) marker;
     
            // DrawSnapLine(snapPoint, uiState, region);
            DrawLabel(snapPoint, uiState, region);
        }
     
     
        static void DrawLabel(QlistMarker point, MarkerUIStates uiState, MarkerOverlayRegion region)
        {
            if (string.IsNullOrEmpty(point.stateName))
                return;
     
            var colorScale = uiState.HasFlag(MarkerUIStates.Selected) ? 1.0f : 0.85f;
     
            var textStyle = EditorStyles.whiteMiniLabel;
            s_Temp.text = point.stateName;
     
            var labelRect = region.markerRegion;
            labelRect.width = textStyle.CalcSize(s_Temp).x + 5;
            labelRect.x -= labelRect.width; // bring it to the left
            var shadowRect = Rect.MinMaxRect(labelRect.xMin + 1, labelRect.yMin + 1, labelRect.xMax + 1, labelRect.yMax + 1);
     
            var oldColor = GUI.color;
            // GUI.color = Color.white * colorScale;
            // GUI.Label(shadowRect, s_Temp, textStyle);
            GUI.color = Color.black;
            GUI.Label(labelRect, s_Temp, textStyle);
            GUI.color = oldColor;
        }
     
        // static void DrawSnapLine(QlistMarker snapPoint, MarkerUIStates uiState, MarkerOverlayRegion region)
        // {
        //     if (snapPoint.snapLine == QlistMarker.SnapLine.None)
        //         return;
     
     
        //     var collapsed = uiState.HasFlag(MarkerUIStates.Collapsed);
        //     if (collapsed && snapPoint.snapLine == QlistMarker.SnapLine.NotCollapsed)
        //         return;
     
        //     float offset = collapsed ? 7: 15;
        //     var color = snapPoint.snapLineColor;
        //     if (uiState.HasFlag(MarkerUIStates.Selected))
        //     {
        //         color = color * 1.5f;
        //     }
     
        //     var r = new Rect(region.markerRegion.center.x - 0.5f,
        //         region.markerRegion.min.y + offset,
        //         1.0f,
        //         region.timelineRegion.height
        //     );
     
        //     var oldColor = GUI.color;
        //     GUI.color = color;
        //     GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill);
     
        //     if (snapPoint.drawHighlight)
        //     {
        //         var previousTime = region.startTime;
        //         foreach (var m in snapPoint.parent.GetMarkers())
        //         {
        //             if (m.time < snapPoint.time && m is QlistMarker)
        //                 previousTime = Math.Max(m.time, previousTime);
        //         }
     
        //         if (previousTime != snapPoint.time)
        //         {
        //             Rect highlightRect = region.markerRegion;
        //             highlightRect.xMin = ToPixel(region, previousTime);
        //             highlightRect.xMax = region.markerRegion.center.x;
        //             highlightRect.height = 2;
        //             GUI.DrawTexture(highlightRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
        //         }
        //     }
     
        //     GUI.color = oldColor;
        // }
     
        static float ToPixel(MarkerOverlayRegion region, double time)
        {
            var p = (time - region.startTime) / (region.endTime - region.startTime);
            return region.timelineRegion.x + region.timelineRegion.width * (float) p;
        }
     
    }
}