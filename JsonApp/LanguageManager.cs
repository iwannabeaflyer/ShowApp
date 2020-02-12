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
    /**    - Add Globalisation  https://www.agiledeveloper.com/articles/LocalizingDOTNet.pdf
    //     (language tag) https://docs.microsoft.com/nl-nl/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
Find
\"alternative\" for looking for the alternative name\n\"english\" for looking for the english name\n\"genres\" for looking in the genres \n\"description\" for looking in the description \n\"watched\" for looking if you watched it \n\"ending\" for looking if it has an ending \n\"exit\" to stop searching
Edit
\"alternative\" for editing the alternative name\n\"english\" for editing the english name\n\"episodes\" for editing the amount of episodes\n\"description\" for editing the description\n\"genres\" for editing its genres\n\"notes\" for editing the notes\n\"score\" for editing the score\n\"runtime\" for editing the runtime\n\"watched\" for editing if you have watched it\n\"ending\" for editing if it has an ending\n\"exit\" for stop editing
Main
\"add\" for adding a new item\n"\"remove\" for removing an excisting item\n\"find\" for finding items\n\"edit\" for editing an excisting item\n\"save\" for saving the excisting items\n\"load\" for loading items from a file\n\"exit\" exit for stopping the program\n

    */

    class LanguageManager
    {
        private static ResourceManager resourceManager;

        public LanguageManager()
        {
            resourceManager = new ResourceManager("JsonApp.strings", System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static void SetCulture(string specifiedCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(specifiedCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(specifiedCulture);
            resourceManager = new ResourceManager("JsonApp.strings", System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static void Display(string specifiedCulture)
        {
            Console.WriteLine("---->" + specifiedCulture);
            SetCulture(specifiedCulture);
            resourceManager = new ResourceManager("JsonApp.strings", System.Reflection.Assembly.GetExecutingAssembly());
            Console.WriteLine(resourceManager.GetString("welcome"));
            Console.WriteLine(resourceManager.GetString("thanks"));
            Console.WriteLine(resourceManager.GetString("bye"));
        }

        public static string GetTranslation(string cul, string s)
        {
            SetCulture(cul);
            resourceManager = new ResourceManager("JsonApp.strings", System.Reflection.Assembly.GetExecutingAssembly());
            return resourceManager.GetString(s);
        }

        public static string GetTranslation(string s)
        {
            return resourceManager.GetString(s);
        }
    }
}
