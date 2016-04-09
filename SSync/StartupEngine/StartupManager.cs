using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSync.StartupEngine
{
    /// <summary>
    /// This class let you invoke 'public static' methods at program startup by using the [StartupInvoke] Attribute.
    /// </summary>
    public class StartupManager
    {
        /// <summary>
        /// Called When an method is loaded.
        /// </summary>
        public static event Action<StartupInvokeType, string> OnItemLoading = null;

        /// <summary>
        /// Called when a method thrown an error
        /// </summary>
        public static event Action<String, Exception> OnErrorThrown = null;

        /// <summary>
        /// Called when all methods with the [StartupInvoke] Attribute are loaded.
        /// </summary>
        public static event Action<TimeSpan> OnStartupEnded = null;
       
        /// <summary>
        /// Initialize all the public static method containing the [StartupInvoke] attributes.
        /// </summary>
        /// <param name="startupAssembly"></param>
        public static void Initialize(Assembly startupAssembly)
        {
            Stopwatch watch = Stopwatch.StartNew();

            foreach (var pass in Enum.GetValues(typeof(StartupInvokeType)))
            {
                foreach (var item in startupAssembly.GetTypes())
                {
                    var methods = item.GetMethods().ToList().FindAll(x => x.GetCustomAttribute(typeof(StartupInvoke), false) != null);
                    var attributes = methods.ConvertAll<KeyValuePair<StartupInvoke, MethodInfo>>(x => new KeyValuePair<StartupInvoke, MethodInfo>(x.GetCustomAttribute(typeof(StartupInvoke), false) as StartupInvoke, x));

                    var concerned = attributes.FindAll(x => x.Key.Type == (StartupInvokeType)pass);
                    foreach (var data in concerned)
                    {
                        if (!data.Key.Hided)
                        {
                            OnItemLoading(data.Key.Type, data.Key.Name);
                        }
                        Delegate del = Delegate.CreateDelegate(typeof(Action), data.Value);
                        try
                        {
                            del.DynamicInvoke();
                        }
                        catch (Exception ex)
                        {
                            OnErrorThrown(data.Key.Name, ex);
                            return;
                        }
                    }


                }
            }
            watch.Stop();
            OnStartupEnded(watch.Elapsed);
        }
    }
}
