using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UrlExtender.Objects {
    public class Config {
        public readonly string rootUrl; // the root URL for the API on the internet-facing site (the local one is localhost/api/)
        public readonly string uiUrl;   // the URL for the webpage that provides a user interface for the API. This is the response (a redirect) to an HTTP GET
                                        // to rootUrl. If you're not using this API for a web app, you can have this redirect to a home page or leave it blank
        public readonly string magicWord;
        public readonly string dbFile;

        public Config() {
            const string prefix = "/srv/urlExtender/";

            string configFile = File.ReadAllText(prefix + "config.txt");
            configFile = Regex.Replace(configFile, @"\t|\n|\r|", "");

            string[] elements = configFile.Split(';');
            Dictionary<string, string> config = new Dictionary<string, string>();
            foreach(string element in elements) {
                try {
                    string[] keyVal = element.Split(" ");
                    config.Add(keyVal[0], keyVal[1]);
                }
                catch (IndexOutOfRangeException) {
                    continue;
                }
            }

            try {
                rootUrl = config["rootUrl"];
                magicWord = config["magicWord"];
                dbFile = prefix + config["dbFile"];
            }
            catch (KeyNotFoundException e) {
                throw new KeyNotFoundException($"Make sure that you have a valid config file with all the correct options set ({e.Message})");
            }

            try {
                uiUrl = config["uiUrl"];
            }
            catch (KeyNotFoundException) {
                uiUrl = null;
            }
        }
    }
}