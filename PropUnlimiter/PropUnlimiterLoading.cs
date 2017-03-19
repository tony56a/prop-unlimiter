using ICities;
using LitJson;
using System;
using PropUnlimiter.Utils;
using Harmony;
using System.Reflection;

namespace PropUnlimiter
{
    public class PropUnlimiterLoading : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            try
            {
                JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(Convert.ToDouble(obj)));
                JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));
            }
            catch (NullReferenceException e)
            {
                LoggerUtils.Log("Failure at jsonmapper!");
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {

            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || 
                mode == LoadMode.NewGameFromScenario || mode == LoadMode.NewScenarioFromGame ||
                mode == LoadMode.LoadScenario || mode == LoadMode.NewScenarioFromMap )
            {
                // do stuff I guess
                try
                {

                }
                catch (Exception ex)
                {
                    LoggerUtils.LogException(ex);
                }

                // Patch all applicable methods
                try
                {
                    var harmony = HarmonyInstance.Create("com.PropUnlimiter");
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                catch (Exception ex)
                {
                    LoggerUtils.LogException(ex);
                }
            }
        }

        public override void OnLevelUnloading()
        {
            
        }
    }
}
