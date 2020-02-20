namespace JsonApp
{
    class Constants
    {
        public const byte MIN = 0;
        public const byte MAX = 100;
        public const byte JSON_FIELDS = 9;
        public const byte MINUTES = 60;

        public const string APPLICATION_NAME = "json creator";

        //All but MAL use '+' to seperate search words, MAL uses %20 or white spaces
        public const string GOOGLE = "https://www.google.com/search?q=";
        public const string MAL_ALL = "https://myanimelist.net/search/all?q=";
        public const string MAL_CATAGORY = "https://myanimelist.net/";
        public const string MAL_ANIME = "anime.php?q=";
        public const string MAL_CHARACTER = "character.php?q=";
        public const string MAL_MANGA = "manga.php?q=";
        public const string IMDB = "https://www.imdb.com/find?q=";
        public const string YOUTUBE = "https://www.youtube.com/results?search_query=";
        public const string WIKIPEDIA = "https://en.wikipedia.org/w/index.php?search=";
    }
}
