using ColossalFramework;
using ColossalFramework.Math;
using LitJson;
using PropUnlimiter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PropUnlimiter.Manager
{

    /// <summary>
    /// A container for Unlimited props, wraps around the actual prop instance itself,
    /// and a dict of string to floats for extra values ( more precise position, etc )
    /// </summary>
    public class PropContainer
    {
        /// <summary>
        /// The prop instance itself
        /// </summary>
        public PropInstance propInstance;

        /// <summary>
        /// The extras dictionary, holds values used by other mods
        /// </summary>
        public Dictionary<string, float> extras = new Dictionary<string, float>();
    }

    class PropUnlimiterManager : Singleton<PropUnlimiterManager>
    {
         /// <summary>
        /// Dictionary of a "grid" of values on the map that correspond to the props in that grid
        /// </summary> 
        private Dictionary<int, List<PropContainer>> props = new Dictionary<int, List<PropContainer>>();

        public Dictionary<int, List<PropContainer>> Props { get => props; }

        /// <summary>
        /// Place a prop in alternate prop store, based on the propInfo ID, intended for use in serialization
        /// </summary>
        /// <param name="infoIndex">ID of prop</param>
        /// <param name="position"> position of prop</param>
        /// <param name="angle"> angle of prop </param>
        /// <param name="single"> some kind of instance flag???</param>
        public void SetUnlimitedProp(int infoIndex, Vector3 position, float angle, bool single)
        {
            PropInfo prefab = PrefabCollection<PropInfo>.GetPrefab((uint)infoIndex);
            SetUnlimitedProp(prefab, position, angle, single);
        }
        /// <summary>
        ///  Place a prop in alternate prop store
        /// </summary>
        /// <param name="info">Prop info(mesh, availability flags, etc)</param>
        /// <param name="position"> position of prop</param>
        /// <param name="angle"> angle of prop </param>
        /// <param name="single"> some kind of instance flag???</param>
        public void SetUnlimitedProp(PropInfo info, Vector3 position, float angle, bool single)
        {
            PropInstance newProp = new PropInstance();
            newProp.m_flags = ((ushort)PropInstance.Flags.Created | 32768 | 16384);
            newProp.Info = info;
            newProp.Single = single;
            newProp.Blocked = false;
            newProp.Position = position;
            newProp.Angle = angle;
            newProp.m_infoIndex = (ushort)info.m_prefabDataIndex;

            // Find grid, then add prop
            int gridVal = PropUtils.calcGrid(position);
            if (!props.ContainsKey(gridVal))
            {
                props[gridVal] = new List<PropContainer>();
            }

            PropContainer container = new PropContainer();
            container.propInstance = newProp;
            props[gridVal].Add(container);
        }

        /// <summary>
        /// Place a prop in prop store with prop info. Intended for deserialization only
        /// </summary>
        /// <param name="wrapper">Prop info</param>
        public void SetUnlimitedProp(PropWrapper wrapper)
        {
            PropInstance newProp = new PropInstance();
            PropInfo prefab = PrefabCollection<PropInfo>.GetPrefab((uint)wrapper.infoIndex);

            newProp.m_flags = ((ushort)PropInstance.Flags.Created | 32768 | 16384);
            newProp.Info = prefab;
            newProp.Single = wrapper.single;
            newProp.Blocked = false;
            newProp.m_posX = (short)wrapper.positionX;
            newProp.m_posY = (ushort)wrapper.positionY;
            newProp.m_posZ = (short)wrapper.positionZ;

            newProp.Angle = wrapper.angle;
            newProp.m_infoIndex = (ushort)wrapper.infoIndex;

            // Find grid, then add prop
            int gridVal = PropUtils.calcGrid(newProp.Position);
            if (!props.ContainsKey(gridVal))
            {
                props[gridVal] = new List<PropContainer>();
            }

            PropContainer container = new PropContainer();
            if(wrapper.extraJson != null)
            {
                container.extras = JsonMapper.ToObject<Dictionary<string, float>>(wrapper.extraJson);
            }
            container.propInstance = newProp;
            props[gridVal].Add(container);
        }

        /// <summary>
        /// Delete a prop, given the prop, and the grid it belongs to
        /// </summary>
        /// <param name="gridKey">The grid value the prop is in</param>
        /// <param name="container">The container to delete</param>
        public void DeleteProp(int gridKey, PropContainer container)
        {
            if (PropUnlimiterManager.instance.props.ContainsKey(gridKey))
            {
                List<PropContainer> list = PropUnlimiterManager.instance.props[gridKey];
                list.Remove(container);
            }
        }

        /// <summary>
        /// Raycast for props, based on original propmanager raycast
        /// </summary>
        /// <param name="input">Input ray for search</param>
        /// <param name="gridKey">grid position the prop is in</param>
        /// <param name="prop">the prop itself</param>
        /// <returns>whether or not a prop was found</returns>
        public bool RaycastUnlimitedProps(Ray input, out int gridKey, out PropContainer prop)
        {
            Vector3 origin = input.origin;
            Vector3 normalized = input.direction.normalized;
            Vector3 destination = input.origin + normalized * Camera.main.farClipPlane;
            Segment3 ray = new Segment3(origin, destination);

            Bounds bounds = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(17280f, 1152f, 17280f));
            if (ray.Clip(bounds))
            {
                // Here be grid calculation voodoo
                Vector3 vector3_1 = ray.b - ray.a;
                int num1 = (int)(ray.a.x / 64.0 + 135.0);
                int num2 = (int)(ray.a.z / 64.0 + 135.0);
                int num3 = (int)(ray.b.x / 64.0 + 135.0);
                int num4 = (int)(ray.b.z / 64.0 + 135.0);
                float num5 = Mathf.Abs(vector3_1.x);
                float num6 = Mathf.Abs(vector3_1.z);
                int num7;
                int num8;
                if (num5 >= num6)
                {
                    num7 = vector3_1.x <= 0.0 ? -1 : 1;
                    num8 = 0;
                    if (num5 != 0.0)
                    {
                        vector3_1 *= 64f / num5;
                    }
                }
                else
                {
                    num7 = 0;
                    num8 = vector3_1.z <= 0.0 ? -1 : 1;
                    if (num6 != 0.0)
                    {
                        vector3_1 *= 64f / num6;
                    }
                }
                Vector3 vector3_2 = ray.a;
                Vector3 vector3_3 = ray.a;
                int a1 = num1;
                int a2 = num2;
                float t1 = 2f;
                float num9 = 10000f;

                int retValgridKey = -1;
                PropContainer retValInstance = new PropContainer();
                do
                {
                    Vector3 vector3_4 = vector3_3 + vector3_1;
                    int num10;
                    int num11;
                    int num12;
                    int num13;
                    if (num7 != 0)
                    {
                        num10 = a1 == num1 && num7 > 0 || a1 == num3 && num7 < 0 ? Mathf.Max((int)(((double)vector3_4.x - 72.0) / 64.0 + 135.0), 0) : Mathf.Max(a1, 0);
                        num11 = a1 == num1 && num7 < 0 || a1 == num3 && num7 > 0 ? Mathf.Min((int)(((double)vector3_4.x + 72.0) / 64.0 + 135.0), 269) : Mathf.Min(a1, 269);
                        num12 = Mathf.Max((int)(((double)Mathf.Min(vector3_2.z, vector3_4.z) - 72.0) / 64.0 + 135.0), 0);
                        num13 = Mathf.Min((int)(((double)Mathf.Max(vector3_2.z, vector3_4.z) + 72.0) / 64.0 + 135.0), 269);
                    }
                    else
                    {
                        num12 = a2 == num2 && num8 > 0 || a2 == num4 && num8 < 0 ? Mathf.Max((int)(((double)vector3_4.z - 72.0) / 64.0 + 135.0), 0) : Mathf.Max(a2, 0);
                        num13 = a2 == num2 && num8 < 0 || a2 == num4 && num8 > 0 ? Mathf.Min((int)(((double)vector3_4.z + 72.0) / 64.0 + 135.0), 269) : Mathf.Min(a2, 269);
                        num10 = Mathf.Max((int)(((double)Mathf.Min(vector3_2.x, vector3_4.x) - 72.0) / 64.0 + 135.0), 0);
                        num11 = Mathf.Min((int)(((double)Mathf.Max(vector3_2.x, vector3_4.x) + 72.0) / 64.0 + 135.0), 269);
                    }

                    //Go over grid values, and iterate over list of props to see if there's a prop in the relevant region
                    for (int index1 = num12; index1 <= num13; ++index1)
                    {
                        for (int index2 = num10; index2 <= num11; ++index2)
                        {
                            int searchGrid = index1 * 270 + index2;
                            List<PropContainer> list = GetPropsInGrid(searchGrid);

                            if (list != null)
                            {
                                
                                foreach (PropContainer listProp in list)
                                {
                                    float t2;
                                    float targetSqr;
                                    if (listProp.propInstance.RayCast(0, ray, out t2, out targetSqr) && ((double)t2 < (double)t1 - 9.99999974737875E-05 || (double)t2 < (double)t1 + 9.99999974737875E-05 && (double)targetSqr < (double)num9))
                                    {
                                        t1 = t2;
                                        num9 = targetSqr;
                                        retValgridKey = searchGrid;
                                        retValInstance = listProp;
                                    }

                                }
                            }

                        }
                    }
                    vector3_2 = vector3_3;
                    vector3_3 = vector3_4;
                    a1 += num7;
                    a2 += num8;
                }
                while ((a1 <= num3 || num7 <= 0) && (a1 >= num3 || num7 >= 0) && ((a2 <= num4 || num8 <= 0) && (a2 >= num4 || num8 >= 0)));
                if ((double)t1 != 2.0)
                {
                    gridKey = retValgridKey;
                    prop = retValInstance;
                    return true;
                }

            }

            //If no prop found, then reset values
            gridKey = -1;
            prop = new PropContainer();
            return false;

        }

        /// <summary>
        /// Gets all props in the grid specified by gridKey, or null if nothing there
        /// </summary>
        /// <param name="gridKey">The grid to get props from</param>
        /// <returns>A list of props in the grid, or null if none available</returns>
        public List<PropContainer> GetPropsInGrid(int gridKey)
        {
            return props.ContainsKey(gridKey) ? props[gridKey] : null;
        }

        /// <summary>
        /// Gets all Prop Unlimiter props
        /// </summary>
        /// <returns>A list of all Prop Unlimiter props</returns>
        public List<PropContainer> GetAllProps()
        {
            List<PropContainer> retVal = new List<PropContainer>();

            foreach (int gridKey in props.Keys)
            {
                retVal.AddRange(GetPropsInGrid(gridKey));
            }
            return retVal;
        }

        /// <summary>
        /// Get all prop infos. Intended for serialization only
        /// </summary>
        /// <returns>Array of all prop infos for serialization</returns>
        public PropWrapper[] GetAllPropWrappers()
        {
            List<PropWrapper> retVal = new List<PropWrapper>();

            foreach( int gridKey in props.Keys)
            {
                retVal.AddRange(GetListPropWrappers(gridKey));
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Get all prop info for a grid. Intended for serialization only
        /// </summary>
        /// <param name="gridKey">The grid to get prop infos from</param>
        /// <returns>A list of props in the specified grid, empty list if grid is not present</returns>
        public List<PropWrapper> GetListPropWrappers(int gridKey)
        {
            List<PropWrapper> retVal = new List<PropWrapper>();
            List<PropContainer> list = GetPropsInGrid(gridKey);
            if(list != null)
            {
                foreach (PropContainer instance in list)
                {
                    retVal.Add(PropWrapper.GeneratePropWrapper(instance));
                }
            }
            return retVal;
        }

        /// <summary>
        /// Populate Prop Unlimiter store, given a list of prop infos 
        /// </summary>
        /// <param name="propInfos">A list of prop infos to load</param>
        public void LoadWrappers(List<PropWrapper> propInfos)
        {
            foreach( PropWrapper wrapper in propInfos )
            {
                SetUnlimitedProp(wrapper);
            }
            PropManager.instance.UpdateProp(0);
            PropManager.instance.UpdatePropRenderer(0, true);
        }
    }
}
