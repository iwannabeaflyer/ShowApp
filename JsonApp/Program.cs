using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Configuration;

namespace JsonApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SettingsTheme(int.Parse(ConfigurationManager.AppSettings.Get("theme")));
            Console.Title = Constants.APPLICATION_NAME;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            LanguageManager.SetCulture(ConfigurationManager.AppSettings.Get("lang"));

            string jsonString = "";
            string cmd;
            bool IsChanged = false;
            JsonObject jsonObject = new JsonObject();

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
                    if(Remove(ref jsonObject)) { IsChanged = true; } //Only change IsChanged when it actually is changed and not blindly grab the value
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
                    Saving(jsonString);
                    IsChanged = false;
                    Console.WriteLine(LanguageManager.GetTranslation("saveComplete"));
                }
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdLoad")))
                {
                    Console.WriteLine(LanguageManager.GetTranslation("load"));
                    jsonObject.Items.Clear();
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
                            string l = SettingsLanguage();
                            LanguageManager.SetCulture(l);
                            config.AppSettings.Settings["lang"].Value = l;
                            config.Save(ConfigurationSaveMode.Modified);
                            break;
                        }
                        else if (cmd.Equals(LanguageManager.GetTranslation("settingsTheme")))
                        {
                            config.AppSettings.Settings["theme"].Value = SettingsTheme();
                            config.Save(ConfigurationSaveMode.Modified);
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
                else if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                {
                    if (IsChanged)
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("unsavedChanges"));
                        if (GetBool()) { Saving(jsonString); IsChanged = false; }
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
        /// Select how you want to save the file
        /// </summary>
        /// <param name="jsonString">string containing the json</param>
        private static void Saving(string jsonString)
        {
            string cmd;
            Console.WriteLine(LanguageManager.GetTranslation("saveMode"));
            if (GetBool())
            {
                string path = "";
                Console.WriteLine(LanguageManager.GetTranslation("directoryPath"));
                cmd = Console.ReadLine();
                if (!Directory.Exists(cmd))
                {
                    Directory.CreateDirectory(cmd);   
                }
                path = Path.GetFullPath(cmd);
                if (!path.EndsWith("\\"))
                {
                    path = path + "\\";
                }
                Save(jsonString, path);
            }
            else
            {
                Save(jsonString);
            }
        }

        /// <summary>
        /// Save a specific jsonstring to a file with a asked for name
        /// </summary>
        /// <param name="json">The json string you want to save in a file</param>
        private static void Save(string json)
        {
            Console.WriteLine(LanguageManager.GetTranslation("saveFileName"));
            string fileName = Console.ReadLine();
            //Saving the file
            File.WriteAllText(fileName + ".json", json);
        }

        /// <summary>
        /// Save a specific jsonstring to a file with an given directory
        /// </summary>
        /// <param name="json">The json string you want to save in a file</param>
        /// <param name="path">The path of where you want to save it</param>
        private static void Save(string json, string path)
        {
            Console.WriteLine(LanguageManager.GetTranslation("saveFileName"));
            string fileName = Console.ReadLine();
            //Saving the file
            File.WriteAllText(path + fileName + ".json", json);
        }

        /// <summary>
        /// Load a specific file into a string using the given fileName, if none is given it will ask for one
        /// </summary>
        /// <param name="fileName">Name of the file you want to look in, can be empty</param>
        /// <returns>The entire content of a file as a string</returns>
        private static string Load()
        {
            StreamReader sr;
            string path = "";
            Console.WriteLine(LanguageManager.GetTranslation("loadMode"));
            if (GetBool())
            {
                Console.WriteLine(LanguageManager.GetTranslation("directoryPath"));
                path = Console.ReadLine();
            }
            Console.WriteLine(LanguageManager.GetTranslation("loadFileName"));
            string fileName = Console.ReadLine();
            string line = "";
            if (Directory.Exists(path))
            {
                fileName = path + fileName;
            }
            else if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine(LanguageManager.GetTranslation("noValidPath"), path);
            }
            try
            {
                using (sr = new StreamReader(fileName + ".json"))
                {
                    line = sr.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return line;
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
        /// Remove an item
        /// </summary>
        /// <param name="jsonObject">ref to the jsonObject</param>
        /// <returns>If there was anything removed</returns>
        private static bool Remove(ref JsonObject jsonObject)
        {
            List<ModelItem> results;
            string cmd;
            bool IsChanged = false;
            bool Start, Question;
            Start = Question = true;
            do
            {
                results = Find(jsonObject);
                if (results == null || results.Count() == 0) continue;
                //acces a specific item
                int elem = AccesItem(results.Count());
                Console.WriteLine(LanguageManager.GetTranslation("removeConfirm"));
                jsonObject.Items[elem].ShowAll();
                do
                {
                    Question = true;
                    Console.WriteLine(LanguageManager.GetTranslation("yesNoOther"));
                    cmd = Console.ReadLine().ToLower();
                    if (cmd.Equals(LanguageManager.GetTranslation("yesShort")) || cmd.Equals(LanguageManager.GetTranslation("yesLong")))
                    {
                        jsonObject.Items.Remove(results.ElementAt(elem));
                        IsChanged = true;
                        Console.WriteLine(LanguageManager.GetTranslation("removeComplete"));
                        Start = false;
                        Question = false;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("otherShort")) || cmd.Equals(LanguageManager.GetTranslation("otherLong")))
                    {
                        Question = false;
                        continue;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("noShort")) || cmd.Equals(LanguageManager.GetTranslation("noLong")))
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("removeCancel"));
                        Start = false;
                        Question = false;
                    }
                } while (Question);
            } while (Start);
            return IsChanged;
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
                    {   //TODO: check if preview should stay, this should look up trailers for both movies and anime, but generaly tv shows don't have a trailers
                        Process.Start(Constants.YOUTUBE + search + "+preview");
                        Console.WriteLine(LanguageManager.GetTranslation("browserOpen"));
                        break;
                    }
                    else if (cmd.Equals(LanguageManager.GetTranslation("cmdExit")))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("invalidCommand"), cmd);
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
            bool innerLoop, outerLoop;
            innerLoop = outerLoop = true;
            //Ask for the command and check wether its a valid one or not
            do
            {
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
            } while (true);
            if (cmd.Equals(LanguageManager.GetTranslation("cmdWatched")) || cmd.Equals(LanguageManager.GetTranslation("cmdEnding")))
            {
                bool term;
                do
                {
                    do
                    {
                        if (cmd.Equals(LanguageManager.GetTranslation("cmdWatched"))) Console.WriteLine(LanguageManager.GetTranslation("findWatched"));
                        else Console.WriteLine(LanguageManager.GetTranslation("findEnding"));
                        term = GetBool();
                        if (term) Console.WriteLine(LanguageManager.GetTranslation("findConfirmYes"));
                        else Console.WriteLine(LanguageManager.GetTranslation("findConfirmNo"));
                        Console.WriteLine(LanguageManager.GetTranslation("findContinue"));
                        if (Console.ReadLine().ToLower().Equals(LanguageManager.GetTranslation("yesShort"))) innerLoop = false;
                    } while (innerLoop);
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
                        if (!GetBool()) { Console.WriteLine(LanguageManager.GetTranslation("findCancel")); outerLoop = false; }
                        else { outerLoop = innerLoop = true; }
                    }
                    else
                    {
                        outerLoop = false;
                    }
                } while (outerLoop);
            }
            else
            {
                string term;
                do
                {
                    do
                    {
                        Console.WriteLine(LanguageManager.GetTranslation("findOther"));
                        term = Console.ReadLine().ToLower();
                        Console.WriteLine(LanguageManager.GetTranslation("findConfirmOther"), term);
                        Console.WriteLine(LanguageManager.GetTranslation("findContinue"));
                        if (Console.ReadLine().ToLower().Equals(LanguageManager.GetTranslation("yesShort"))) innerLoop = false;
                    } while (innerLoop);
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
                        if (!GetBool()) { Console.WriteLine(LanguageManager.GetTranslation("findCancel")); outerLoop = false; }
                        else { outerLoop = innerLoop = true; }
                    }
                    else
                    {
                        outerLoop = false;
                    }
                } while (outerLoop);
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
            {
                case 1:
                    l = "en";
                    break;
                case 2:
                    l = "nl";
                    break;
                default:
                    l = "en"; //default file is written in english
                    break;
            }
            //ConfigurationManager.AppSettings.Set("lang", l); //Doesn't seem to change the app.config file.
            Console.WriteLine(LanguageManager.GetTranslation("newLanguage") + l);
            return l;
        }

        /// <summary>
        /// Change the theme of the application
        /// </summary>
        private static string SettingsTheme()
        {
            Console.WriteLine(LanguageManager.GetTranslation("setTheme"));
            Console.WriteLine(LanguageManager.GetTranslation("themes"));
            return SettingsTheme(MinMax(GetNumber(), 1, 3));
        }

        /// <summary>
        /// Change the theme of the application based on the index
        /// </summary>
        /// <param name="i">index number of the language</param>
        private static string SettingsTheme(int i)
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
            return i.ToString();
            //ConfigurationManager.AppSettings.Set("theme", i.ToString()); //Doesn't seem to change app.Config
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
                    Console.WriteLine(LanguageManager.GetTranslation("NaN"), cmd);
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
            if (string.IsNullOrEmpty(s)) return null;
            return s.First().ToString().ToUpper() + s.Substring(1);
        }
    }
}