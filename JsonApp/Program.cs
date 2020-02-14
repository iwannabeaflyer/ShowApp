using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Configuration;

/*** DONE
Changed ModelItem constructor so it can generate ModelItems from a json file with an already valid construction
Changed ModelItem serializer so it can generate valid json strings from ModelItems
Functionality of it reminding the user that he/she made a change and ask to save if they want to save it before closing the app (bool has to change within Main, edit doesn't have to change)
Changed Edit to include the new data
Add functionality to convert runtime to an hh:mm or hh:mm format (only needed on All())
Changed some of the variables to be slightly more memory efficient. (using smaller data types where possible aswell as using unsigned types instead of signed)
Changed Find to include watched and hasend options (refactored)
Added Globalisation (language tag)
Added the ability to open a webbrowser to look more information up
*/
/*** internet stuff
  for google    "https://www.google.com/search?q=" and then the word or words where spaces are indicated by '+' so one+punch+man
  for mal       "https://myanimelist.net/search/all?q=" and then word or words where spaces are indicated by "%20" so one%20punch%20man (or replace %20 with a white space)
  but specific  "https://myanimelist.net/anime.php?q=" where anime.php can be replaced with manga.php, character.php and other categories
  imdb          "https://www.imdb.com/find?q=" and then word or words where spaces are indicated by '+' so iron+man+3 (https://www.imdb.com/find?q= + search + &ref_=nv_sr_sm) is also possible
  youtube       "https://www.youtube.com/results?search_query=" and then word or words where spaces are indicated by '+' so one+punch+man
  wikipedia     "https://en.wikipedia.org/w/index.php?search=" and then word or words where spaces are indicated by '+' so one+punch+man  
    Process.Start("https://www.youtube.com/watch?v=eI3ldLdCXKs"); //opens the default webbrowser with time for polka on yt
 */
/*** TODO
    - App.Config file doesn't save the changes when trying to apply them, but remembers them for the current session
    - Think if i want to change it so that in Notes you can also use the ':' character without the program breaking. (change the way of deserialization of the last 2 fields)
     - Think if you want a trashcan functionality for removing, this should help users recover accidentaly deleted shows but you will not be able to clear the memory while the program is open, 
    aswell as that what is in the trashcan will be deleted once the program exits. Furthermore there is already a check to try to prevent users from deleting things they didn't want to delete.
     */

namespace JsonApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SettingsTheme(int.Parse(ConfigurationManager.AppSettings.Get("theme")));
            Console.Title = Constants.APPLICATION_NAME;
            string jsonString = "";
            string cmd;
            bool IsChanged = false;
            JsonObject jsonObject = new JsonObject();
            
            LanguageManager.SetCulture(ConfigurationManager.AppSettings.Get("lang"));

            Console.WriteLine(LanguageManager.GetTranslation("programStart"));
            while (true)
            {
                cmd = Console.ReadLine().ToLower();
                if (cmd.Equals(LanguageManager.GetTranslation("cmdAdd")))
                {
                    jsonObject.Items.Add(Add());
                    IsChanged = true;
                    Console.WriteLine(LanguageManager.GetTranslation("addComplete"));
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdRemove")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("remove"));
                    List<ModelItem> results;
                    do
                    {
                        Start:
                        results = Find(jsonObject);
                        if (results == null || results.Count() == 0) continue;
                        //acces a specific item
                        int elem = AccesItem(results.Count());
                        Console.WriteLine(LanguageManager.GetTranslation("removeConfirm"));
                        jsonObject.Items[elem].ShowAll();
                        Question:
                        Console.WriteLine(LanguageManager.GetTranslation("yesNoOther"));
                        cmd = Console.ReadLine().ToLower();
                        if (cmd.Equals(LanguageManager.GetTranslation("yesShort")) || cmd.Equals(LanguageManager.GetTranslation("yesLong")))
                        {
                            jsonObject.Items.Remove(results.ElementAt(elem));
                            IsChanged = true;
                            Console.WriteLine(LanguageManager.GetTranslation("removeComplete"));
                            break;
                        }
                        else if (cmd.Equals(LanguageManager.GetTranslation("otherShort")) || cmd.Equals(LanguageManager.GetTranslation("otherLong")))
                        {
                            goto Start;
                        }
                        else if (cmd.Equals(LanguageManager.GetTranslation("noShort")) || cmd.Equals(LanguageManager.GetTranslation("noLong")))
                        {
                            Console.WriteLine(LanguageManager.GetTranslation("removeCancel"));
                            break;
                        }
                        else { goto Question; }
                    } while (true);
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdFind")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("find"));
                    List<ModelItem> results = Find(jsonObject);
                    if (results == null || results.Count() == 0) continue;
                    //acces a specific item
                    int elem = AccesItem(results.Count());
                    results.ElementAt(elem).ShowAll();
                    //Ask if the user wants to open a webbrowser for more information
                    OpenBrowser(results.ElementAt(elem).ReturnName());
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdEdit")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("edit"));
                    //find certain items
                    List<ModelItem> results = Find(jsonObject);
                    if (results == null || results.Count() == 0) continue;
                    //acces a specific item
                    int elem = AccesItem(results.Count());
                    ModelItem edit = results.ElementAt(elem);
                    //Ask if the user wants to open a webbrowser for more information
                    OpenBrowser(edit.ReturnName());
                    //edit specific field(s)
                    IsChanged = Edit(ref edit);
                    Console.WriteLine(LanguageManager.GetTranslation("editComplete"));
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdSave")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("save"));
                    jsonString = Serialize(jsonObject);
                    Save(jsonString);
                    IsChanged = false;
                    Console.WriteLine(LanguageManager.GetTranslation("saveComplete"));
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdLoad")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("load"));
                    jsonString = Load();
                    jsonObject.Items = Deserialize(jsonString);
                    Console.WriteLine(LanguageManager.GetTranslation("loadComplete"));
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdSettings")))
                {
                    LanguageManager.GetTranslation("settings");
                    do
                    {
                        cmd = Console.ReadLine();
                        if (cmd.Equals(LanguageManager.GetTranslation("settingsLanguage")))
                        {
                            LanguageManager.SetCulture(SettingsLanguage());
                            break;
                        }
                        else if (cmd.Equals(LanguageManager.GetTranslation("settingsTheme")))
                        {
                            SettingsTheme();
                            break;
                        }
                        else if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine(LanguageManager.GetTranslation("invalidCommand"), cmd);
                            Console.WriteLine(LanguageManager.GetTranslation("settingsOptions"));
                        }
                    } while (true);
                }
                else if (cmd.Equals("clear"))
                {
                    Console.Clear();
                }
                else if (cmd.Equals("polka")) { Process.Start("https://www.youtube.com/watch?v=eI3ldLdCXKs"); }
                else if (cmd.Equals("lonely")) { Process.Start("https://www.youtube.com/watch?v=unOSs2P-An0"); }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                {
                    if (IsChanged)
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("unsavedChanges"));
                        if (GetBool()) { Save(jsonString); IsChanged = false; }
                    }
                    break;
                }
                else
                {
                    Console.WriteLine(LanguageManager.GetTranslation("invalidCommand"), cmd);
                    Console.WriteLine(LanguageManager.GetTranslation("mainOptions"));
                }
            }
            Console.WriteLine(LanguageManager.GetTranslation("programEnd"));
            Console.ReadKey();
        }

        /// <summary>
        /// Get the element within the its range
        /// </summary>
        /// <param name="count">Count or Lenght of the Structure</param>
        /// <returns>int that is within the bounds of a structure</returns>
        private static int AccesItem(int count)
        {
            int elem;
            do
            {
                Console.WriteLine(LanguageManager.GetTranslation("accesItem"));
                string cmd = Console.ReadLine();
                if (Int32.TryParse(cmd, out elem))
                {
                    if (0 < elem && elem <= count)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("accesOutofRange"));
                    }
                }
                else
                {
                    Console.WriteLine(LanguageManager.GetTranslation("NaN"), cmd);
                }
            } while (true);
            return --elem;
        }

        /// <summary>
        /// Deserialize a json string to a list of ModelItems
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns>List of ModelItems made using the json string</returns>
        private static List<ModelItem> Deserialize(string jsonString)
        {
            List<ModelItem> result = new List<ModelItem>();
            //split jsonString into modelitems objects
            jsonString = jsonString.TrimStart('{').TrimEnd('}');
            string[] temp = jsonString.Split('{', '}');
            //Add the important information to a model
            for (int i = 1; i < temp.Length; i += 2)
            {
                result.Add(new ModelItem(temp[i]));
            }
            return result;
        }

        /// <summary>
        /// Serialize the JsonObject to a json string
        /// </summary>
        /// <param name="jsonObject">JsonObject you want to serialize</param>
        /// <returns>The given JsonObject as string</returns>
        private static string Serialize(JsonObject jsonObject)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append('{');
            foreach (ModelItem item in jsonObject.Items)
            {
                jsonBuilder.Append("\"");
                jsonBuilder.Append(item.SerializeModel());
                jsonBuilder.Append(",");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 2, 2);
            jsonBuilder.Append('}', 2);
            return jsonBuilder.ToString();
        }

        /// <summary>
        /// Save a specific jsonstring to a file with the name, if the name is not given it will ask for one
        /// </summary>
        /// <param name="json">The json string you want to save in a file</param>
        /// <param name="fileName">The name of the file</param>
        private static void Save(string json)
        {
            Console.WriteLine(LanguageManager.GetTranslation("saveFileName"));
            string fileName = Console.ReadLine();
            //Saving the file
            File.WriteAllText(fileName + ".json", json);
        }

        /// <summary>
        /// Load a specific file into a string using the given fileName, if none is given it will ask for one
        /// </summary>
        /// <param name="fileName">Name of the file you want to look in, can be empty</param>
        /// <returns>The entire content of a file as a string</returns>
        private static string Load()
        {
            Console.WriteLine(LanguageManager.GetTranslation("loadFileName"));
            string fileName = Console.ReadLine();
            //Read the file
            return File.ReadAllText(fileName + ".json");
        }

        /// <summary>
        /// Add a new ModelItem from scratch
        /// </summary>
        /// <returns>The newly created ModelItem</returns>
        private static ModelItem Add()
        {
            Console.WriteLine(LanguageManager.GetTranslation("addNewItem"));
            ModelItem item = new ModelItem();
            //fill each section invidualy
            Console.WriteLine(LanguageManager.GetTranslation("assignEnglish"));
            item.EnName = FirstToUpper(Console.ReadLine());
            Console.WriteLine(LanguageManager.GetTranslation("assignAlternative"));
            item.AltName = FirstToUpper(Console.ReadLine());
            //Ask if the user wants to open a webbrowser for more information
            OpenBrowser(item.ReturnName());
            Console.WriteLine(LanguageManager.GetTranslation("assignEpisodes"));
            item.Episodes = (ushort)GetNumber();
            Console.WriteLine(LanguageManager.GetTranslation("assignDescription"));
            item.Description = FirstToUpper(Console.ReadLine().Replace("\"", ""));
            Console.WriteLine(LanguageManager.GetTranslation("assignGenres"));
            foreach (string s in Console.ReadLine().Split(' '))
            {
                item.Genres.Add(FirstToUpper(s));
            }
            Console.WriteLine(LanguageManager.GetTranslation("assignScore"));
            item.Score = (byte)MinMax(GetNumber(), Constants.MIN, Constants.MAX);
            Console.WriteLine(LanguageManager.GetTranslation("assignRuntime"));
            item.RunTime = (uint)GetNumber() * item.Episodes;
            Console.WriteLine(LanguageManager.GetTranslation("assignWatched"));
            item.Watched = GetBool();
            Console.WriteLine(LanguageManager.GetTranslation("assignEnding"));
            item.HasEnd = GetBool();
            Console.WriteLine(LanguageManager.GetTranslation("assignNotes"));
            item.Notes = Console.ReadLine();

            return item;
        }

        /// <summary>
        /// Ask to open a webbrowser to find more information
        /// </summary>
        /// <param name="search">Name of the item or show</param>
        private static void OpenBrowser(string search)
        {
            string cmd;
            Console.WriteLine(LanguageManager.GetTranslation("browserConfirm"));
            if (GetBool())
            {
                Console.WriteLine(LanguageManager.GetTranslation("browserInput"));
                do
                {
                    cmd = Console.ReadLine().ToLower();
                    if (cmd.Equals(LanguageManager.GetTranslation("mal")))
                    {
                        Process.Start(Constants.MAL_ALL + search);
                        Console.WriteLine(LanguageManager.GetTranslation("browserOpen"));
                        break;
                    }
                    //MAL is the only site that accepts white spaces or %20, others need '+' between search words
                    search = search.Replace(" ", "+");
                    if (cmd.Equals(LanguageManager.GetTranslation("google")))
                    {
                        Process.Start(Constants.GOOGLE + search);
                        Console.WriteLine(LanguageManager.GetTranslation("browserOpen"));
                        break;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("wikipedia")))
                    {
                        Process.Start(Constants.WIKIPEDIA + search);
                        Console.WriteLine(LanguageManager.GetTranslation("browserOpen"));
                        break;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("imdb")))
                    {
                        Process.Start(Constants.IMDB + search);
                        Console.WriteLine(LanguageManager.GetTranslation("browserOpen"));
                        break;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("youtube")))
                    {   //TODO: maybe add trailer so it searches specificly for it
                        Process.Start(Constants.YOUTUBE + search);
                        Console.WriteLine(LanguageManager.GetTranslation("browserOpen"));
                        break;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("invalidCommand"));
                        Console.WriteLine(LanguageManager.GetTranslation("browserOptions"));
                    }
                } while (true);
            }
        }

        /// <summary>
        /// Edit a specific part of the json
        /// </summary>
        /// <param name="item">ref to the ModelItem that you want to edit</param>
        /// <returns>true if it has changed</returns>
        private static bool Edit(ref ModelItem item)
        {
            //TODO: ask if the user wants to open an webbrowser with the given title to look up for more information
            bool b = false;
            Console.WriteLine(LanguageManager.GetTranslation("editField"));
            string cmd;
            while (true)
            {
                cmd = Console.ReadLine().ToLower();
                if (cmd.Equals(LanguageManager.GetTranslation("cmdAlternative")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignAlternative"));
                    item.AltName = FirstToUpper(Console.ReadLine());
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdEnglish")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignEnglish"));
                    item.EnName = FirstToUpper(Console.ReadLine());
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdEpisodes")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignEpisodes"));
                    item.Episodes = (ushort)GetNumber();
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdDescription")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignDescription"));
                    item.Description = FirstToUpper(Console.ReadLine().Replace("\"", ""));
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdGenres")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignGenres"));
                    List<string> temp = new List<string>();
                    foreach (string s in Console.ReadLine().Split(' '))
                    {
                        temp.Add(FirstToUpper(s));
                    }
                    item.Genres = temp;
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdNotes")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignNotes"));
                    item.Notes = Console.ReadLine();
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdScore")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignScore"));
                    item.Score = (byte)MinMax(GetNumber(), Constants.MIN, Constants.MAX);
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdRuntime")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignRuntime"));
                    item.RunTime = (uint)(item.Episodes * GetNumber());
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdWatched")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignWatched"));
                    item.Watched = GetBool();
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdEnding")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("assignEnding"));
                    item.HasEnd = GetBool();
                    b = true;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                {
                    item.ShowAll();
                    break;
                }
                else
                {
                    Console.WriteLine(cmd + LanguageManager.GetTranslation("invalidCommand"));
                    Console.WriteLine(LanguageManager.GetTranslation("editOptions"));
                }
            }
            return b;
        }

        /// <summary>
        /// Find all ModelItems that contain an a term
        /// </summary>
        /// <param name="jsonObject">JsonObject which contains the json you want to search in</param>
        /// <returns>A ModelItem List that contains all items that contains the search term</returns>
        private static List<ModelItem> Find(JsonObject jsonObject)
        {
            List<ModelItem> result = new List<ModelItem>();
            string cmd;

            while (true)
            {
                //Ask for the command and check wether its a valid one or not
                Console.WriteLine(LanguageManager.GetTranslation("findField"));
                cmd = Console.ReadLine().ToLower();

                if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("findCancel"));
                    return null;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdAlternative")) || 
                    cmd.Equals(LanguageManager.GetTranslation("cmdEnglish")) || 
                    cmd.Equals(LanguageManager.GetTranslation("cmdGenres")) || 
                    cmd.Equals(LanguageManager.GetTranslation("cmdDescription")) || 
                    cmd.Equals(LanguageManager.GetTranslation("cmdWatched")) || 
                    cmd.Equals(LanguageManager.GetTranslation("cmdEnding"))) { break; }
                else
                {
                    Console.WriteLine(LanguageManager.GetTranslation("invalidCommand"), cmd);
                    Console.WriteLine(LanguageManager.GetTranslation("findOptions"));
                }
            }
            if (cmd.Equals(LanguageManager.GetTranslation("cmdWatched")) || cmd.Equals(LanguageManager.GetTranslation("cmdEnding")))
            {
                bool term;
                Boolterm:
                do
                {
                    if (cmd.Equals(LanguageManager.GetTranslation("cmdWatched"))) Console.WriteLine(LanguageManager.GetTranslation("findWatched"));
                    else Console.WriteLine(LanguageManager.GetTranslation("findEnding"));
                    term = GetBool();
                    if (term) Console.WriteLine(LanguageManager.GetTranslation("findConfirmYes"));
                    else Console.WriteLine(LanguageManager.GetTranslation("findConfirmNo"));
                    Console.WriteLine(LanguageManager.GetTranslation("findContinue"));
                    if (Console.ReadLine().ToLower().Equals(LanguageManager.GetTranslation("yesShort"))) break;
                } while (true);
                //Start searching
                if (cmd.Equals(LanguageManager.GetTranslation("cmdWatched")))
                {
                    for (int i = 0; i < jsonObject.Items.Count; i++)
                    {
                        if (jsonObject.Items[i].Watched == term) { result.Add(jsonObject.Items[i]); }
                    }
                }
                else
                {
                    for (int i = 0; i < jsonObject.Items.Count; i++)
                    {
                        if (jsonObject.Items[i].HasEnd == term) { result.Add(jsonObject.Items[i]); }
                    }
                }
                //Check if the list is empty
                if (result.Count == 0)
                {
                    Console.WriteLine(LanguageManager.GetTranslation("findNoResult"), cmd, BoolToAnwser(term));
                    if (!GetBool()) { Console.WriteLine(LanguageManager.GetTranslation("findCancel")); }
                    else { goto Boolterm; }
                }
            }
            else
            {
                string term;
                Stringterm:
                do
                {
                    Console.WriteLine(LanguageManager.GetTranslation("findOther"));
                    term = Console.ReadLine().ToLower();
                    Console.WriteLine(LanguageManager.GetTranslation("findConfirmOther"), term);
                    Console.WriteLine(LanguageManager.GetTranslation("findContinue"));
                    if (Console.ReadLine().ToLower().Equals(LanguageManager.GetTranslation("yesShort"))) break;
                } while (true);
                //Start searching
                if (cmd.Equals(LanguageManager.GetTranslation("cmdAlternative")))
                {
                    foreach (ModelItem mi in jsonObject.Items)
                    {
                        if (mi.AltName.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdEnglish")))
                {
                    foreach (ModelItem mi in jsonObject.Items)
                    {
                        if (mi.EnName.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdGenres")))
                {
                    foreach (ModelItem mi in jsonObject.Items)
                    {
                        foreach (string g in mi.Genres)
                        {
                            if (g.ToLower() == term) result.Add(mi);
                        }
                    }
                }
                else
                {
                    foreach (ModelItem mi in jsonObject.Items)
                    {
                        if (mi.Description.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                //Check if the list is empty
                if (result.Count == 0)
                {
                    Console.WriteLine(LanguageManager.GetTranslation("findNoResult"), cmd, term);
                    if (!GetBool()) { Console.WriteLine(LanguageManager.GetTranslation("findCancel")); }
                    else { goto Stringterm; }
                }
            }
            for (int i = 0; i < result.Count; i++)
            {
                Console.WriteLine(i + 1);
                Console.WriteLine(result[i].ReturnName());
            }
            return result;
        }

        /// <summary>
        /// Change the language of the application
        /// </summary>
        /// <returns>Language Tag of the CultureInfo</returns>
        private static string SettingsLanguage()
        {
            string l;
            Console.WriteLine(LanguageManager.GetTranslation("setLanguage"));
            Console.WriteLine(LanguageManager.GetTranslation("languages"));
            switch (GetNumber())
            {   case 1 :
                    l = "en";
                    break;
                case 2 :
                    l = "nl";
                    break;
                default:
                    l = "en"; //default file is written in english
                    break;
            }
            ConfigurationManager.AppSettings.Set("lang", l); //Doesn't seem to change the app.config file.
            Console.WriteLine(LanguageManager.GetTranslation("newLanguage") + l);
            return l;
        }
        
        /// <summary>
        /// Change the theme of the application
        /// </summary>
        private static void SettingsTheme()
        {
            Console.WriteLine(LanguageManager.GetTranslation("setTheme"));
            Console.WriteLine(LanguageManager.GetTranslation("themes"));
            SettingsTheme(MinMax(GetNumber(), 1, 3));
        }

        /// <summary>
        /// Change the theme of the application based on the index
        /// </summary>
        /// <param name="i">index number of the language</param>
        private static void SettingsTheme(int i)
        {
            switch (i)
            {
                case 1:
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case 2:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 3:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            Console.Clear();
            ConfigurationManager.AppSettings.Set("theme", i.ToString()); //Doesn't seem to change app.Config
        }

        /// <summary>
        /// Make sure a number is between min and max
        /// </summary>
        /// <param name="i">number to clamp</param>
        /// <returns>int between min and max</returns>
        private static int MinMax(int i, int min, int max)
        {
            return Math.Min(Math.Max(min, i), max);
        }

        /// <summary>
        /// Get a valid bool from an input
        /// </summary>
        /// <returns>bool extracted from an input</returns>
        private static bool GetBool()
        {
            bool b;
            do
            {
                Console.WriteLine(LanguageManager.GetTranslation("yesNo"));
                string cmd = Console.ReadLine().ToLower();
                if (cmd.Equals(LanguageManager.GetTranslation("yesShort")) || cmd.Equals(LanguageManager.GetTranslation("yesLong")))
                {
                    b = true;
                    break;
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("noShort")) || cmd.Equals(LanguageManager.GetTranslation("noLong")))
                {
                    b = false;
                    break;
                }
                Console.WriteLine(LanguageManager.GetTranslation("invalidCommand"), cmd);
            } while (true);
            return b;
        }

        /// <summary>
        /// Get a valid number from an input
        /// </summary>
        /// <returns>int exctracted from an input</returns>
        private static int GetNumber()
        {
            int num;
            do
            {
                string cmd = Console.ReadLine().Replace(',', '.');
                if (Int32.TryParse(cmd, out num)) break;
                else
                {
                    Console.WriteLine(LanguageManager.GetTranslation("NaN"),cmd);
                }
            } while (true);
            return num;
        }

        /// <summary>
        /// Transform a bool to string
        /// </summary>
        /// <param name="b">bool to transform</param>
        /// <returns>the given bool as a string</returns>
        public static string BoolToAnwser(bool b)
        {   // condition ? true : false
            return b ? "yes" : "no";
        }

        /// <summary>
        /// Change a string so the first letter is uppercase
        /// </summary>
        /// <param name="s">string to change</param>
        /// <returns>the given string with the first character as uppercase</returns>
        private static string FirstToUpper(string s)
        {
            return s.First().ToString().ToUpper() + s.Substring(1);
        }
    }
}
