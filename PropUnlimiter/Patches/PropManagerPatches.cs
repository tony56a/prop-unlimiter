﻿using ColossalFramework.Math;
using Harmony;
using PropUnlimiter.Manager;
using PropUnlimiter.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PropUnlimiter.Patches
{
    [HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("CreateProp")]
    class PropCreatePatch
    {
        public static bool Prefix(ref PropInfo info, ref Vector3 position, ref float angle, ref bool single)
        {
            if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                return true;
            }
            else
            {
#if DEBUG
                string str = String.Format("Invoking prop at:{0}, angle:{1}, info:{2}", position, angle, info.GetLocalizedTitle());
                LoggerUtils.Log(str);
#endif
                PropUnlimiterManager.instance.SetUnlimitedProp(info.m_prefabDataIndex, position, angle, single);
                PropTool.DispatchPlacementEffect(position, true);
                PropManager.instance.UpdateProp(0);
                PropManager.instance.UpdatePropRenderer(0, true);
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("EndRenderingImpl")]
    class PropRenderingPatch
    {
        /// <summary>
        /// Intended for internal accurate rendering only.
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="container"></param>
        private static void RenderProp(RenderManager.CameraInfo cameraInfo, PropContainer container)
        {
            if ( (container.propInstance.m_flags & 68) != 0)
            {
                return;
            }
            PropInfo info = container.propInstance.Info;
            Vector3 inaccuratePosition = container.propInstance.Position;
            Vector3 position = new Vector3(container.extras["accx"], inaccuratePosition.y, container.extras["accz"]);
            if (!cameraInfo.CheckRenderDistance(position, info.m_maxRenderDistance) || !cameraInfo.Intersect(position, info.m_generatedInfo.m_size.y * info.m_maxScale))
            {
                return;
            }
            float angle = container.propInstance.Angle;
            Randomizer r = new Randomizer(0);
            float scale = info.m_minScale + (float)((double)r.Int32(10000U) * ((double)info.m_maxScale - (double)info.m_minScale) * 9.99999974737875E-05);
            Color color = info.GetColor(ref r);
            Vector4 objectIndex = new Vector4(1f / 512f, 1f / 384f, 0.0f, 0.0f);
            InstanceID id = new InstanceID();
            if (info.m_requireHeightMap)
            {
                Texture _HeightMap;
                Vector4 _HeightMapping;
                Vector4 _SurfaceMapping;
                TerrainManager.instance.GetHeightMapping(position, out _HeightMap, out _HeightMapping, out _SurfaceMapping);
                PropInstance.RenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, true, _HeightMap, _HeightMapping, _SurfaceMapping);
            }
            else
            {
                PropInstance.RenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, true);
            }
        }

        public static void Postfix(ref RenderManager.CameraInfo cameraInfo)
        {
            ItemClass.Availability availability = ToolManager.instance.m_properties.m_mode;
            FastList<RenderGroup> fastList = RenderManager.instance.m_renderedGroups;
            int num1 = 1 << LayerMask.NameToLayer("Props") | 1 << RenderManager.instance.lightSystem.m_lightLayer;
            for (int index1 = 0; index1 < fastList.m_size; ++index1)
            {
                RenderGroup renderGroup = fastList.m_buffer[index1];
                if ((renderGroup.m_instanceMask & num1) != 0)
                {
                    int minX = renderGroup.m_x * 270 / 45;
                    int minZ = renderGroup.m_z * 270 / 45;
                    int maxX = (renderGroup.m_x + 1) * 270 / 45 - 1;
                    int maxZ = (renderGroup.m_z + 1) * 270 / 45 - 1;
                    for (int index2 = minZ; index2 <= maxZ; ++index2)
                    {
                        for (int index3 = minX; index3 <= maxX; ++index3)
                        {
                            int gridKey = index2 * 270 + index3;
                            List<PropContainer> list = PropUnlimiterManager.instance.GetPropsInGrid(gridKey);

                            if (list != null)
                            {
                                foreach (PropContainer instance in list)
                                {
                                    if ( instance.extras.ContainsKey("accx") && instance.extras.ContainsKey("accy") && instance.extras.ContainsKey("accz"))
                                    {
                                        RenderProp(cameraInfo, instance);
                                    }
                                    else
                                    {
                                        instance.propInstance.RenderInstance(cameraInfo, 0, renderGroup.m_instanceMask);
                                    }
                                }
                            }

                        }
                    }
                }
            }

            for (int index = 0; index < PrefabCollection<PropInfo>.PrefabCount(); ++index)
            {
                PropInfo prefab = PrefabCollection<PropInfo>.GetPrefab((uint)index);
                if (prefab != null && prefab.m_lodCount != 0)
                {
                    PropInstance.RenderLod(cameraInfo, prefab);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("AfterTerrainUpdate")]
    class PropTerrainUpdatedPatch
    {
        public static void Postfix(TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            
            float minX = heightArea.m_min.x;
            float minZ = heightArea.m_min.z;
            float maxX = heightArea.m_max.x;
            float maxZ = heightArea.m_max.z;

            int minGridX = Mathf.Max((int)(((double)minX - 8.0) / 64.0 + 135.0), 0);
            int minGridZ = Mathf.Max((int)(((double)minZ - 8.0) / 64.0 + 135.0), 0);
            int maxGridX = Mathf.Min((int)(((double)maxX + 8.0) / 64.0 + 135.0), 269);
            int maxGridZ = Mathf.Min((int)(((double)maxZ + 8.0) / 64.0 + 135.0), 269);

            for (int index1 = minGridZ; index1 <= maxGridZ; ++index1)
            {
                for (int index2 = minGridX; index2 <= maxGridX; ++index2)
                {
                    int gridKey = index1 * 270 + index2;
                    List<PropContainer> list = PropUnlimiterManager.instance.GetPropsInGrid(gridKey);

                    if (list != null)
                    {
                        for(int i =0; i<list.Count; i++)
                        {
                            PropInstance instance = list[i].propInstance;
                            instance.TerrainUpdated(0, minX, minZ, maxX, maxZ);
                            Vector3 position = instance.Position;
                            position.y = TerrainManager.instance.SampleDetailHeight(position);
                            ushort num = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, (int)ushort.MaxValue);
                            instance.m_posY = num;
                            list[i].propInstance = instance;
                            PropManager.instance.UpdateProp(0);
                            PropManager.instance.UpdatePropRenderer(0, true);
                        }
                        
                    }

                }
            }

        }
    }
}
