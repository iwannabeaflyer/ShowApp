using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Resources;


/***
 DONE
Changed ModelItem constructor so it can generate ModelItems from a json file with an already valid construction
Changed ModelItem serializer so it can generate valid json strings from ModelItems
Functionality of it reminding the user that he/she made a change and ask to save if they want to save it before closing the app (bool has to change within Main, edit doesn't have to change)
Changed Edit to include the new data
Add functionality to convert runtime to an hh:mm or hh:mm format (only needed on All())
Changed some of the variables to be slightly more memory efficient. (using smaller data types where possible aswell as using unsigned types instead of signed)
Changed Find to include watched and hasend options (refactored)

TODO

    - Internet intergration in looking for looking up more information about a show. https://stackoverflow.com/questions/6305388/how-to-launch-a-google-chrome-tab-with-specific-url-using-c-sharp
    Could be used when using Find or even with Add and Edit.
  for google    "https://www.google.com/search?q=" and then the word or words where spaces are indicated by '+' so one+punch+man
  
  for mal       "https://myanimelist.net/search/all?q=" and then word or words where spaces are indicated by "%20" so one%20punch%20man (or replace %20 with a white space)
  but specific  "https://myanimelist.net/anime.php?q=" where anime.php can be replaced with manga.php, character.php and other categories

  imdb          "https://www.imdb.com/find?q=" and then word or words where spaces are indicated by '+' so iron+man+3 (https://www.imdb.com/find?q= + search + &ref_=nv_sr_sm) is also possible

  youtube       "https://www.youtube.com/results?search_query=" and then word or words where spaces are indicated by '+' so one+punch+man

  wikipedia     "https://en.wikipedia.org/w/index.php?search=" and then word or words where spaces are indicated by '+' so one+punch+man

    - Think if you want a trashcan functionality for removing, this should help users recover accidentaly deleted shows but you will not be able to clear the memory while the program is open, 
    aswell as that what is in the trashcan will be deleted once the program exits. Furthermore there is already a check to try to prevent users from deleting things they didn't want to delete.

    - Think if i want to change it so that in Notes you can also use the ':' character without the program breaking.

    - Add Globalisation https://www.agiledeveloper.com/articles/LocalizingDOTNet.pdf
     */

namespace JsonApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = Constants.APPLICATION_NAME;
            string jsonString = "";
            string cmd;
            bool IsChanged = false;
            JsonObject jsonObject = new JsonObject();

            LanguageManager.Display("en-US");
            LanguageManager.Display("en-GB");
            LanguageManager.Display("fr-FR");
            LanguageManager.Display("es-MX");

            Console.WriteLine("Starting Program");
            while (true)
            {
                cmd = Console.ReadLine().ToLower();
                if (cmd.Equals("add"))
                {
                    jsonObject.Items.Add(Add());
                    IsChanged = true;
                    Console.WriteLine("Adding complete");
                }
                else if (cmd.Equals("remove"))
                {
                    Console.WriteLine("Removing");
                    List<ModelItem> results;
                    do
                    {
                        Start:
                        results = Find(jsonObject);
                        if (results == null || results.Count() == 0) continue;

                        int elem = AccesItem(results.Count());
                        Console.WriteLine("Are you sure you want to remove:");
                        jsonObject.Items[elem].ShowAll();
                        Question:
                        Console.WriteLine("Yes / No / Other");
                        cmd = Console.ReadLine().ToLower();
                        if (cmd.Equals("y") || cmd.Equals("yes"))
                        {
                            jsonObject.Items.Remove(results.ElementAt(elem));
                            IsChanged = true;
                            Console.WriteLine("Removing complete");
                            break;
                        }
                        else if (cmd.Equals("o") || cmd.Equals("other"))
                        {
                            goto Start;
                        }
                        else if (cmd.Equals("n") || cmd.Equals("no"))
                        {
                            Console.WriteLine("Canceling removal");
                            break;
                        }
                        else
                        {
                            goto Question;
                        }

                    } while (true);
                }
                else if (cmd.Equals("find"))
                {
                    Console.WriteLine("Finding");
                    List<ModelItem> results = Find(jsonObject);
                    if (results == null || results.Count() == 0) continue;

                    //acces a specific item
                    int elem = AccesItem(results.Count());

                    results.ElementAt(elem).ShowAll();

                    //TODO ask if the user wants to open a webbrowser for more information about this item
                }
                else if (cmd.Equals("edit"))
                {
                    Console.WriteLine("Editing");
                    //find certain items
                    List<ModelItem> results = Find(jsonObject);
                    if (results == null || results.Count() == 0) continue;

                    //acces a specific item
                    int elem = AccesItem(results.Count());
                    ModelItem edit = results.ElementAt(elem);
                    //edit specific field(s)
                    IsChanged = Edit(ref edit);
                    Console.WriteLine("Editing complete");
                }
                else if (cmd.Equals("save"))
                {
                    Console.WriteLine("Saving");
                    jsonString = Serialize(jsonObject);
                    Save(jsonString);
                    IsChanged = false;
                    Console.WriteLine("Saving complete");
                }
                else if (cmd.Equals("load"))
                {
                    Console.WriteLine("Loading");
                    jsonString = Load();
                    jsonObject.Items = Deserialize(jsonString);
                    Console.WriteLine("Loading complete");
                }
                else if (cmd.Equals("clear"))
                {
                    Console.Clear();
                }
                else if (cmd.Equals("polka")) { Process.Start("https://www.youtube.com/watch?v=eI3ldLdCXKs"); }
                else if (cmd.Equals("lonely")) { Process.Start("https://www.youtube.com/watch?v=unOSs2P-An0"); }
                else if (cmd.Equals("exit"))
                {
                    if (IsChanged)
                    {
                        Console.WriteLine("The file has been changed but not saved, do you want to save it?");
                        if (GetBool()) { Save(jsonString); IsChanged = false; }
                    }
                    break;
                }
                else
                {
                    Console.WriteLine(cmd + " isn't a valid command");
                    Console.WriteLine("\"add\" for adding a new item\n" +
                        "\"remove\" for removing an excisting item\n" +
                        "\"find\" for finding items\n" +
                        "\"edit\" for editing an excisting item\n" +
                        "\"save\" for saving the excisting items\n" +
                        "\"load\" for loading items from a file\n" +
                        "\"exit\" exit for stopping the program\n");
                }
            }
            Console.WriteLine("Ending Program");
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
                Console.WriteLine("What item do you want to acces?");
                string cmd = Console.ReadLine();
                if (Int32.TryParse(cmd, out elem))
                {
                    if (0 < elem && elem <= count)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Index is out of range");
                    }
                }
                else
                {
                    Console.WriteLine(cmd + " isn't a valid number please try again");
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
            Console.WriteLine("What should the file be named?");
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
            Console.WriteLine("What is the name of the file?");
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
            Console.WriteLine("Adding a new item");
            ModelItem item = new ModelItem();
            //fill each section invidualy
            Console.WriteLine("Enter its English name of the show");
            item.EnName = FirstToUpper(Console.ReadLine());
            Console.WriteLine("Enter a Alternative name of the show if it has one");
            item.AltName = FirstToUpper(Console.ReadLine());
            //TODO: give the user an option to use the just given name to look it up for more information with a webbrowser
            Console.WriteLine("Enter the amount of episodes of the show");
            item.Episodes = (ushort)GetNumber();
            Console.WriteLine("Enter the description of the show");
            item.Description = FirstToUpper(Console.ReadLine().Replace("\"", ""));
            Console.WriteLine("Enter the genres of the show");
            foreach (string s in Console.ReadLine().Split(' '))
            {
                item.Genres.Add(FirstToUpper(s));
            }
            Console.WriteLine("Enter the score of the show");
            item.Score = (byte)MinMax(GetNumber(), Constants.MIN, Constants.MAX);
            Console.WriteLine("Enter the run time per episode");
            item.RunTime = (uint)GetNumber() * item.Episodes;
            Console.WriteLine("Enter if you have watched it");
            item.Watched = GetBool();
            Console.WriteLine("Enter if it has an ending");
            item.HasEnd = GetBool();
            Console.WriteLine("Enter additional notes");
            item.Notes = Console.ReadLine();

            return item;
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
            Console.WriteLine("What field do you want to edit?");
            string cmd;
            while (true)
            {
                cmd = Console.ReadLine().ToLower();
                if (cmd.Equals("alternative"))
                {
                    Console.WriteLine("Enter its Alternative name");
                    item.AltName = FirstToUpper(Console.ReadLine());
                    b = true;
                }
                else if (cmd.Equals("english"))
                {
                    Console.WriteLine("Enter its English name");
                    item.EnName = FirstToUpper(Console.ReadLine());
                    b = true;
                }
                else if (cmd.Equals("episodes"))
                {
                    Console.WriteLine("Enter the amount of episodes");
                    item.Episodes = (ushort)GetNumber();
                    b = true;
                }
                else if (cmd.Equals("description"))
                {
                    Console.WriteLine("Enter the description");
                    item.Description = FirstToUpper(Console.ReadLine().Replace("\"", ""));
                    b = true;
                }
                else if (cmd.Equals("genres"))
                {
                    Console.WriteLine("Enter the genres");
                    List<string> temp = new List<string>();
                    foreach (string s in Console.ReadLine().Split(' '))
                    {
                        temp.Add(FirstToUpper(s));
                    }
                    item.Genres = temp;
                    b = true;
                }
                else if (cmd.Equals("notes"))
                {
                    Console.WriteLine("Enter its notes");
                    item.Notes = Console.ReadLine();
                    b = true;
                }
                else if (cmd.Equals("score"))
                {
                    Console.WriteLine("Enter its scoring");
                    item.Score = (byte)MinMax(GetNumber(), Constants.MIN, Constants.MAX);
                    b = true;
                }
                else if (cmd.Equals("runtime"))
                {
                    Console.WriteLine("Enter the runtime per episode");
                    item.RunTime = (uint)(item.Episodes * GetNumber());
                    b = true;
                }
                else if (cmd.Equals("watched"))
                {
                    Console.WriteLine("Enter if you have watched it");
                    item.Watched = GetBool();
                    b = true;
                }
                else if (cmd.Equals("ending"))
                {
                    Console.WriteLine("Enter if it has an ending");
                    item.HasEnd = GetBool();
                    b = true;
                }
                else if (cmd.Equals("exit"))
                {
                    item.ShowAll();
                    break;
                }
                else
                {
                    Console.WriteLine(cmd + " isn't a valid command");
                    Console.WriteLine("\"alternative\" for editing the alternative name\n" +
                        "\"english\" for editing the english name\n" +
                        "\"episodes\" for editing the amount of episodes\n" +
                        "\"description\" for editing the description\n" +
                        "\"genres\" for editing its genres\n" +
                        "\"notes\" for editing the notes\n" +
                        "\"score\" for editing the score\n" +
                        "\"runtime\" for editing the runtime\n" +
                        "\"watched\" for editing if you have watched it\n" +
                        "\"ending\" for editing if it has an ending\n" +
                        "\"exit\" for stop editing");
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
                Console.WriteLine("What field do you want to search in?");
                cmd = Console.ReadLine();

                if (cmd.Equals("exit"))
                {
                    Console.WriteLine("Cancel Search");
                    return null;
                }
                else if (cmd.Equals("alternative") || cmd.Equals("english") || cmd.Equals("genres") || cmd.Equals("description") || cmd.Equals("watched") || cmd.Equals("ending")) { break; }
                else
                {
                    Console.WriteLine(cmd + " isn't a valid command");
                    Console.WriteLine("\"alternative\" for looking for the alternative name\n " +
                        "\"english\" for looking for the english name\n " +
                        "\"genres\" for looking in the genres \n" +
                        "\"description\" for looking in the description \n" +
                        "\"watched\" for looking if you watched it \n" +
                        "\"ending\" for looking if it has an ending \n" +
                        "\"exit\" to stop searching");
                }
            }
            if (cmd.Equals("watched") || cmd.Equals("ending"))
            {
                bool term;
                Boolterm:
                do
                {
                    if (cmd.Equals("watched")) Console.WriteLine("Have you watched it?");
                    else Console.WriteLine("Does it have an ending");
                    term = GetBool();
                    if (term) Console.WriteLine("Are you sure you want to search using: yes ?");
                    else Console.WriteLine("Are you sure you want to search using: no ?");
                    Console.WriteLine("Type \"y\" to continue");
                    if (Console.ReadLine().ToLower().Equals("y")) break;
                } while (true);
                //Start searching
                if (cmd.Equals("watched"))
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
                    Console.WriteLine("There hasn't been any item found using: in {0} with the term {1} do you want to use a different term?", cmd, BoolToAnwser(term));
                    if (!GetBool()) { Console.WriteLine("Canceling search"); }
                    else { goto Boolterm; }
                }
            }
            else
            {
                string term;
                Stringterm:
                do
                {
                    Console.WriteLine("On do you want to use to search?");
                    term = Console.ReadLine().ToLower();
                    Console.WriteLine("Are you sure you want to search using: {0} ?", term);
                    Console.WriteLine("Type \"y\" to continue");
                    if (Console.ReadLine().ToLower().Equals("y")) break;
                } while (true);
                //Start searching
                if (cmd.Equals("alternative"))
                {
                    foreach (ModelItem mi in jsonObject.Items)
                    {
                        if (mi.AltName.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                else if (cmd.Equals("english"))
                {
                    foreach (ModelItem mi in jsonObject.Items)
                    {
                        if (mi.EnName.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                else if (cmd.Equals("genres"))
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
                    Console.WriteLine("There hasn't been any item found using: in {0} with the term {1} do you want to use a different term?", cmd, term);
                    if (!GetBool()) { Console.WriteLine("Canceling search"); }
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
                Console.WriteLine("Yes / No");
                string cmd = Console.ReadLine().ToLower();
                if (cmd.Equals("y") || cmd.Equals("yes"))
                {
                    b = true;
                    break;
                }
                else if (cmd.Equals("n") || cmd.Equals("no"))
                {
                    b = false;
                    break;
                }
                Console.WriteLine(cmd + " isn't a valid anwser please enter a valid anwser");
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
                    Console.WriteLine(cmd + " isn't a valid number please enter a valid number");
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
