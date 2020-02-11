using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;


/***
 DONE
Changed ModelItem constructor so it can generate ModelItems from a json file with an already valid construction
Changed ModelItem serializer so it can generate valid json strings from ModelItems
Functionality of it reminding the user that he/she made a change and ask to save if they want to save it before closing the app (bool has to change within Main, edit doesn't have to change)
Changed Edit to include the new data
Add functionality to convert runtime to an hh:mm or hh:mm format (only needed on All())

TODO
- Change Find to include watched and hasend options (probably have to rework the entire function)

- Internet intergration in looking for looking up more information about a show. https://stackoverflow.com/questions/6305388/how-to-launch-a-google-chrome-tab-with-specific-url-using-c-sharp
  for google    "https://www.google.com/search?q=" and then the word or words where spaces are indicated by '+' so time+for+polka
  
  for mal       "https://myanimelist.net/search/all?q=" and then word or words where spaces are indicated by "%20" so the%20rising%20of%20the%20shield%20hero (or replace %20 with a white space)
  but specific  "https://myanimelist.net/anime.php?q=" where anime.php can be replaced with manga.php, character.php and other categories

  imdb          "https://www.imdb.com/find?q=" and then word or words where spaces are indicated by '+' so iron+man+3 (https://www.imdb.com/find?q= + search + &ref_=nv_sr_sm) is also possible

  youtube       "https://www.youtube.com/results?search_query=" and then wor or words where spaces are indicated by '+' so time+for+polka

- Think if you want a trashcan functionality for removing, this should help users recover accidentaly deleted shows but you will not be able to clear the memory while the program is open, 
    aswell as that what is in the trashcan will be deleted once the program exits. Furthermore there is already a check to try to prevent users from deleting things they didn't want to delete.
     */

namespace JsonApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "json creator";
            string jsonString = "";
            string cmd;
            bool IsChanged = false;
            JsonObject jsonObject = new JsonObject();

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
                    //edit specific field
                    edit = Edit(edit);
                    IsChanged = true;
                    Console.WriteLine("Editing complete");
                }
                else if (cmd.Equals("save"))
                {
                    Console.WriteLine("Saving");
                    jsonString = Serialize(jsonObject);
                    IsChanged = Save(jsonString);
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
                else if (cmd.Equals("exit"))
                {
                    if (IsChanged)
                    {
                        Console.WriteLine("The file has been changed but not saved, do you want to save it?");
                        if(GetBool()) { IsChanged = Save(jsonString); }
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
        private static bool Save(string json)
        {
            Console.WriteLine("What should the file be named?");
            string fileName = Console.ReadLine();
            //Saving the file
            File.WriteAllText(fileName + ".json", json);
            return false;
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
            Console.WriteLine("Enter the amount of episodes of the show");
            item.Episodes = GetNumber();
            Console.WriteLine("Enter the description of the show");
            item.Description = FirstToUpper(Console.ReadLine().Replace("\"",""));
            Console.WriteLine("Enter the genres of the show");
            foreach (string s in Console.ReadLine().Split(' '))
            {
                item.Genres.Add(FirstToUpper(s));
            }
            Console.WriteLine("Enter the score of the show");
            item.Score = MinMax(GetNumber());
            Console.WriteLine("Enter the run time per episode");
            item.RunTime = GetNumber() * item.Episodes;
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
        /// <param name="item">ModelItem that you want to edit</param>
        /// <returns>The edited item</returns>
        private static ModelItem Edit(ModelItem item)
        {
            Console.WriteLine("What field do you want to edit?");
            string cmd;
            while (true)
            {
                cmd = Console.ReadLine().ToLower();
                if (cmd.Equals("alternative"))
                {
                    Console.WriteLine("Enter its Alternative name");
                    item.AltName = FirstToUpper(Console.ReadLine());
                }
                else if (cmd.Equals("english"))
                {
                    Console.WriteLine("Enter its English name");
                    item.EnName = FirstToUpper(Console.ReadLine());
                }
                else if (cmd.Equals("episodes"))
                {
                    Console.WriteLine("Enter the amount of episodes");
                    item.Episodes = GetNumber();
                }
                else if (cmd.Equals("description"))
                {
                    Console.WriteLine("Enter the description");
                    item.Description = FirstToUpper(Console.ReadLine().Replace("\"",""));
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
                }
                else if (cmd.Equals("notes"))
                {
                    Console.WriteLine("Enter its notes");
                    item.Notes = Console.ReadLine();
                }
                else if (cmd.Equals("score"))
                {
                    Console.WriteLine("Enter its scoring");
                    item.Score = MinMax(GetNumber());
                }
                else if (cmd.Equals("runtime"))
                {
                    Console.WriteLine("Enter the runtime per episode");
                    item.RunTime = item.Episodes * GetNumber();
                }
                else if (cmd.Equals("watched"))
                {
                    Console.WriteLine("Enter if you have watched it");
                    item.Watched = GetBool();
                }
                else if (cmd.Equals("end"))
                {
                    Console.WriteLine("Enter if it has an ending");
                    item.HasEnd = GetBool();
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
                        "\"end\" for editing if it has an end\n" +
                        "\"exit\" for stop editing");
                }
            }
            return item;
        }

        /// <summary>
        /// Find all ModelItems that contain an a term
        /// </summary>
        /// <param name="jObject">JsonObject which contains the json you want to search in</param>
        /// <returns>A ModelItem List that contains all items that contains the search term</returns>
        private static List<ModelItem> Find(JsonObject jObject)
        {
            //TODO: Add a check if wether it needs a yes/no answer or a word to look for, maybe rework this method
            List<ModelItem> result = new List<ModelItem>();
            string cmd, term;
            //go through the list
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
                else if (cmd.Equals("alternative") || cmd.Equals("english") || cmd.Equals("genres") || cmd.Equals("description") || cmd.Equals("watched") || cmd.Equals("end")) { break; }
                else
                {
                    Console.WriteLine(cmd + " isn't a valid command");
                    Console.WriteLine("\"alternative\" for looking for the alternative name\n " +
                        "\"english\" for looking for the english name\n " +
                        "\"genres\" for looking in the genres \n" +
                        "\"description\" for looking in the description \n" +
                        "\"watched\" for looking if you watched it \n" +
                        "\"end\" for looking if it has an ending \n" +
                        "\"exit\" to stop searching");
                }
            }
            do
            {
                while (true)
                {
                    //Check wether if in where you want to search in contains the term
                    Console.WriteLine("On what do you want to search?");
                    term = Console.ReadLine().ToLower();
                    Console.WriteLine("Are you sure you want to search using: " + term + " ?");
                    Console.WriteLine("Type \"y\" to continue");
                    if (Console.ReadLine().ToLower().Equals("y")) break;
                }

                if (cmd.Equals("alternative"))
                {
                    foreach (ModelItem mi in jObject.Items)
                    {
                        if (mi.AltName.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                else if (cmd.Equals("english"))
                {
                    foreach (ModelItem mi in jObject.Items)
                    {
                        if (mi.EnName.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                else if (cmd.Equals("genres"))
                {
                    foreach (ModelItem mi in jObject.Items)
                    {
                        foreach (string t in mi.Genres)
                        {
                            if (t.ToLower() == term) result.Add(mi);
                        }
                    }
                }
                else if (cmd.Equals("description"))
                {
                    foreach (ModelItem mi in jObject.Items)
                    {
                        if (mi.Description.ToLower().Contains(term)) result.Add(mi);
                    }
                }
                if (result.Count == 0)
                {
                    Console.WriteLine("There hasn't been any item found using: {0} in {1} do you want to use a different term?", term, cmd);
                    Console.WriteLine("Y / N");
                    if (Console.ReadLine().ToUpper().Equals("N")) { Console.WriteLine("Canceling search"); break; }
                }
                else { break; }
            } while (true);

            //Add index numbers to it
            switch (cmd)
            {
                case "alternative":
                    result.ForEach(delegate (ModelItem item)
                    {
                        Console.WriteLine(result.IndexOf(item) + 1);
                        Console.WriteLine(item.ReturnName());
                    });
                    break;
                case "english":
                    result.ForEach(delegate (ModelItem item)
                    {
                        Console.WriteLine(result.IndexOf(item) + 1);
                        Console.WriteLine(item.ReturnName());
                    });
                    break;
                case "genres":
                    result.ForEach(delegate (ModelItem item)
                    {
                        Console.WriteLine(result.IndexOf(item) + 1);
                        Console.WriteLine(item.ReturnName());
                    });
                    break;
                case "description":
                    result.ForEach(delegate (ModelItem item)
                    {
                        Console.WriteLine(result.IndexOf(item) + 1);
                        Console.WriteLine(item.ReturnName());
                    });
                    break;
            }
            return result;
        }

        /// <summary>
        /// Make sure a number is between 0 and 100
        /// </summary>
        /// <param name="i">number to clamp</param>
        /// <returns>int between 0 and 100</returns>
        private static int MinMax(int i)
        {
            return Math.Min(Math.Max(0, i), 100);
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

        private static string FirstToUpper(string s)
        {
            return s.First().ToString().ToUpper() + s.Substring(1);
        }
    }
}
