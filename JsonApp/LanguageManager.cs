using System.Threading;
using System.Globalization;
using System.Resources;

namespace JsonApp
{
    /**    - Add Globalisation  https://www.agiledeveloper.com/articles/LocalizingDOTNet.pdf
        (language tag) https://docs.microsoft.com/nl-nl/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
    */

    class LanguageManager
    {
        private static ResourceManager resourceManager;

        /// <summary>
        /// Sets a culture of the application to change the language of the application
        /// </summary>
        /// <param name="specifiedCulture">language tag of the language</param>
        public static void SetCulture(string specifiedCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(specifiedCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(specifiedCulture);
            resourceManager = new ResourceManager("JsonApp.strings", System.Reflection.Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Get a translation of the field from the language file
        /// </summary>
        /// <param name="s">name of the field of the string</param>
        /// <returns>text out of the language file</returns>
        public static string GetTranslation(string s)
        {
            return resourceManager.GetString(s);
        }
    }
}
