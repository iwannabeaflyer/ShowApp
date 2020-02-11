using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApp
{
    public class ModelItem
    {
        public string EnName { get; set; }          //Name of the show in english
        public string AltName { get; set; }         //an alternative name for the show
        public ushort Episodes { get; set; }        //Amount of episodes (max is 65.535, longest running is ~20.000 episodes which started in 1959)
        public List<string> Genres { get; set; }    //Genres of the show
        public byte Score { get; set; }             //Score of the show (clamped between 0 and 100)
        public uint RunTime { get; set; }           //Total runtime (still can contain 4.294.967.295 min or ~71.582.788 hours or max #episodes with 65.537 minutes per episode)
        public bool Watched { get; set; }           //Seen this show
        public bool HasEnd { get; set; }            //Does it has an ending
        public string Notes { get; set; }           //Additional notes about the show
        public string Description { get; set; }     //Description of the show


        public ModelItem() { Genres = new List<string>(); }
        public ModelItem(string json)
        {
            char[] array = { ':' };
            string[] results = json.Split(array, Constants.JSON_FIELDS);

            EnName = results[1].Split(',')[0].Trim('"');
            AltName = results[2].Split(',')[0].Trim('"');
            Episodes = ushort.Parse(results[3].Split(',')[0].Trim('"'));
            Genres = new List<string>();
            string[] genres = results[4].Trim('[').Replace("]", "").Replace("\"", "").Split(',');
            for (int i = 0; i < genres.Length - 1; i++)
            {
                Genres.Add(genres[i]);
            }
            Score = byte.Parse(results[5].Split(',')[0]);
            RunTime = uint.Parse(results[6].Split(',')[0]);
            Watched = StringToBool(results[7].Split(',')[0]);
            HasEnd = StringToBool(results[8].Split(',')[0]);
            //NOTE: you can make Constants.JSON_FIELDS 9 and then manually split the last result by .Split("\"description\":"); then the first part will be the notes and the second part will
            //have the description left. This should make it so that notes also can have ':' in it.
            Notes = results[9].Split(',')[0].Trim('"');
            Description = results[10].Trim('"');

            /*
            char[] array = { ',' };
            string[] results = json.Split(array, 7);

            AltName = results[0].Split(':')[1].Trim('"');
            EnName = results[1].Split(':')[1].Trim('"');
            Episodes = int.Parse(results[2].Split(':')[1].Trim('"'));
            Score = int.Parse(results[3].Split(':')[1].Trim('"'));
            Notes = results[4].Split(':')[1].Trim('"');

            StringBuilder sb = new StringBuilder();
            sb.Append(results[5].Split(':')[1].TrimStart('"'));
            sb.Append(',');
            sb.Append(results[6].Split(':')[0].Replace("\",\"tags\"", ""));
            Description = sb.ToString();
            Genres = new List<string>();
            string[] genres = results[6].Split(':')[1].Trim('"').Replace("\"", "").Trim('[', ']', '}').Split(',');
            foreach (string genre in genres)
            {
                Genres.Add(genre);
            }
            */
        }

        /// <summary>
        /// Create a json string from this object
        /// </summary>
        /// <returns>This model formatted to a jsonstring</returns>
        public string SerializeModel()
        {
            StringBuilder modelBuilder = new StringBuilder();
            //object name
            modelBuilder.Append(ReturnName());

            //english name pair
            modelBuilder.Append("\",\"enName\":\"");
            modelBuilder.Append(EnName);

            //alternative name pair
            modelBuilder.Append("\":{\"altName\":\"");
            modelBuilder.Append(AltName);

            //episodes pair
            modelBuilder.Append("\",\"episodes\":");
            modelBuilder.Append(Episodes);

            //genres array
            modelBuilder.Append(",\"genre\":[");
            foreach (string s in Genres)
            {
                modelBuilder.Append("\"");
                modelBuilder.Append(s);
                modelBuilder.Append("\",");
            }
            modelBuilder.Remove(modelBuilder.Length - 1, 1);
            modelBuilder.Append("]");

            //score pair
            modelBuilder.Append(",\"score\":");
            modelBuilder.Append(Score);

            //runtime pair
            modelBuilder.Append(",\"runtime\":");
            modelBuilder.Append(RunTime);

            //watched pair
            modelBuilder.Append(",\"watched\":");
            modelBuilder.Append(BoolToString(Watched));

            //hasend pair
            modelBuilder.Append(",\"hasend\":");
            modelBuilder.Append(BoolToString(HasEnd));

            //notes pair
            modelBuilder.Append(",\"notes\":\"");
            modelBuilder.Append(Notes);

            //description pair
            modelBuilder.Append("\",\"description\":\"");
            modelBuilder.Append(Description);

            //finish the json string
            modelBuilder.Append("\"}");
            return modelBuilder.ToString();
        }

        /// <summary>
        /// Transform a bool to string
        /// </summary>
        /// <param name="b">bool to transform</param>
        /// <returns>the given bool as a string</returns>
        public string BoolToAnwser(bool b)
        {   // condition ? true : false
            return b ? "yes" : "no";
        }

        /// <summary>
        /// Convert a string representing a bool to a bool
        /// </summary>
        /// <param name="s">the bool as string</param>
        /// <returns>the bool from the string</returns>
        public bool StringToBool(string s)
        {
            return s.Equals("true");
        }

        /// <summary>
        /// Covert a bool to a string representing a bool
        /// </summary>
        /// <param name="b">bool to represent as a string</param>
        /// <returns>bool converted to a string</returns>
        private string BoolToString(bool b)
        {
            string s;
            if (b) s = "true";
            else s = "false";
            return s;
        }

        /// <summary>
        /// Returns the name of the show first in english else use the alternative name
        /// </summary>
        /// <returns>returns a string with containing the name or empty when it doesn't have a name</returns>
        public string ReturnName()
        {
            string s;
            if (!string.IsNullOrWhiteSpace(EnName)) { s = EnName; }
            else if (!string.IsNullOrWhiteSpace(AltName)) { s = AltName; }
            else { s = ""; Console.WriteLine("Failed to serialize a show since it doesn't have a name"); }
            return s;
        }

        /// <summary>
        /// Shows all info about an item
        /// </summary>
        public void ShowAll()
        {
            Console.WriteLine("English name: " + EnName);
            Console.WriteLine("Alternative name: " + AltName);
            Console.WriteLine("Number of Episodes: " + Episodes);
            Console.WriteLine("Description: " + Description);
            Console.Write("Genres: ");
            foreach (string genre in Genres)
            {
                Console.Write("\"" + genre + "\" ");
            }
            Console.WriteLine("");
            Console.WriteLine("Score: " + Score);
            Console.WriteLine("Watched: " + BoolToAnwser(Watched));
            Console.WriteLine("Run time is {0}:{1} or {2} minutes per episode", RunTime / Constants.MINUTES, RunTime % Constants.MINUTES, RunTime / Episodes);
            Console.WriteLine("Has an ending: " + BoolToAnwser(HasEnd));
            Console.WriteLine("Notes: " + Notes);
            Console.WriteLine("");
        }
    }
}
