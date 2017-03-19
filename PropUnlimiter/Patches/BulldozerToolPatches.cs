using ColossalFramework.UI;
using Harmony;
using PropUnlimiter.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PropUnlimiter.Patches
{
    public static class ContainerHolder
    {
        public static int gridKey = -1;
        public static PropContainer instance = null;
    }
    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("RenderOverlay")]
    public class BulldozerToolOverlayPatch
    {
        static Color toolColor = new Color32(0, 181, 255, 255);

        [HarmonyAfter(new string[] { "com.MarkaRoute" })]
        public static bool Prefix(BulldozeTool __instance, ref RenderManager.CameraInfo cameraInfo)
        {

            if (ContainerHolder.instance != null)
            {
                PropInstance propInstance = ContainerHolder.instance.propInstance;
                float size = Mathf.Max(propInstance.Info.m_generatedInfo.m_size.x, propInstance.Info.m_generatedInfo.m_size.z) * 1;
                Vector3 position = propInstance.Position;
                ++ToolManager.instance.m_drawCallData.m_overlayCalls;
                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, toolColor, position, size, position.y - 100f, position.y + 100f, false, true);

                return false;
            }

            return true;
        }

    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("OnToolGUI")]
    public class BulldozerToolDeletePatch
    {
        [HarmonyAfter(new string[] { "com.MarkaRoute" })]
        public static bool Prefix()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == (int)UIMouseButton.None)
            {

                if (ContainerHolder.instance != null)
                {
                    PropInstance propInstance = ContainerHolder.instance.propInstance;
                    PropUnlimiterManager.instance.DeleteProp(ContainerHolder.gridKey, ContainerHolder.instance);
                    PropTool.DispatchPlacementEffect(propInstance.Position, true);

                    ContainerHolder.instance = null;
                    ContainerHolder.gridKey = -1;
                    return false;
                }

            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("OnToolUpdate")]
    public class BulldozerToolPatch
    {
        [HarmonyAfter(new string[] { "com.MarkaRoute" })]
        public static void Postfix(BulldozeTool __instance)
        {

            if (__instance != null && __instance.enabled)
            {
                Ray currentPosition = Camera.main.ScreenPointToRay(Input.mousePosition);
               
                PropUnlimiterManager.instance.RaycastUnlimitedProps(currentPosition, out ContainerHolder.gridKey, out ContainerHolder.instance);

                // This is probably not needed, since raycast already wipes out the existing values
                if (ContainerHolder.instance == null)
                {
                    ContainerHolder.instance = null;
                    ContainerHolder.gridKey = -1;
                }
             

            }

        }


    }

}
