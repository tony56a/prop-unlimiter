using LitJson;
using PropUnlimiter.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PropUnlimiter.Utils
{
    class PropUtils
    {
        public static int calcGrid(Vector3 position)
        {
            return calcGrid(position.x, position.z);
        }

        public static int calcGrid(float x, float z)
        {
            int computedX = (int)Mathf.Clamp(((x - 8.0f) / 64.0f + 135.0f), 0f, 269f);
            int computedZ = (int)Mathf.Clamp(((z - 8.0f) / 64.0f + 135.0f), 0f, 269f);

            return computedZ * 270 + computedX;
        }
    }

    [Serializable]
    class PropWrapper
    {
        public int infoIndex;
        public int positionX;
        public int positionY;
        public int positionZ;
        public float angle;
        public bool single;
        public string extraJson;

        public static PropWrapper GeneratePropWrapper(PropContainer propContainer)
        {
            PropWrapper retVal = new PropWrapper();
            PropInstance instance = propContainer.propInstance;
            Dictionary<string, float> extras = propContainer.extras;
            retVal.infoIndex = instance.m_infoIndex;
            retVal.positionX = instance.m_posX;
            retVal.positionY = instance.m_posY;
            retVal.positionZ = instance.m_posZ;
            retVal.angle = instance.Angle;
            retVal.single = instance.Single;
            retVal.extraJson = JsonMapper.ToJson(extras);
            return retVal;
        }
    }
}
