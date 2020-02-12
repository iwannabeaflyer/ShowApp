using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Resources;

namespace JsonApp
{
    class LanguageManager
    {
        public static void SetCulture(string specifiedCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(specifiedCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(specifiedCulture);
        }

        public static void Display(string specifiedCulture)
        {
            Console.WriteLine("---->" + specifiedCulture);
            SetCulture(specifiedCulture);
            ResourceManager resourceMgr = new
            ResourceManager("JsonApp.strings", System.Reflection.Assembly.GetExecutingAssembly());
            Console.WriteLine(resourceMgr.GetString("welcome"));
            Console.WriteLine(resourceMgr.GetString("thanks"));
            Console.WriteLine(resourceMgr.GetString("bye"));
        }
    }
}
