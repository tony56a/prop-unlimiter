using ICities;
using PropUnlimiter.Manager;
using PropUnlimiter.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PropUnlimiter
{
    public class PropUnlimiterSerializer : SerializableDataExtensionBase
    {
        private readonly string dataKey = "PropUnlimiter";
        private Object thisLock = new Object();

        public override void OnSaveData()
        {
            LoggerUtils.Log("Saving props");

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream propMemoryStream = new MemoryStream();

            try
            {
                PropWrapper[] propInfos = PropUnlimiterManager.instance.GetAllPropWrappers();
                if (propInfos != null)
                {
                    binaryFormatter.Serialize(propMemoryStream, propInfos);
                    serializableDataManager.SaveData(dataKey, propMemoryStream.ToArray());
                    LoggerUtils.Log("Props have been saved!");

                }
                else
                {
                    LoggerUtils.LogWarning("Couldn't save props, as the array is null!");
                }

            }
            catch (Exception e)
            {
                LoggerUtils.LogError(e);
            }
        }

        public override void OnLoadData()
        {
            LoggerUtils.Log("Loading data");
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            lock (thisLock)
            {

                byte[] loadedPropData = serializableDataManager.LoadData(dataKey);
                LoggerUtils.Log("Props loaded, parsing data");
                if (loadedPropData != null && loadedPropData.Length > 0)
                {
                    MemoryStream memoryStream = new MemoryStream();

                    memoryStream.Write(loadedPropData, 0, loadedPropData.Length);
                    memoryStream.Position = 0;

                    try
                    {
                        PropWrapper[] props = binaryFormatter.Deserialize(memoryStream) as PropWrapper[];

                        if (props != null && props.Length > 0)
                        {
                            PropUnlimiterManager.instance.LoadWrappers(props.ToList());
                        }
                        else
                        {
                            LoggerUtils.LogWarning("Couldn't load props, as the array is null!");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerUtils.LogException(ex);

                    }
                    finally
                    {
                        memoryStream.Close();
                    }
                }
                else
                {
                    LoggerUtils.LogWarning("Found no data to load");
                }
            }
            
        }
    }
}
