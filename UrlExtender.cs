using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UrlExtender.Gzip;
using UrlExtender.Objects;

namespace UrlExtender {
    public static class Extender {
        private static readonly Config config;
        private static Dictionary<int, ExtendedEntry> db;
        private static bool dbLock = false;        

        static Extender() {
            config = new Config();

            if(File.Exists(config.dbFile)) {
                var compressed = File.ReadAllBytes(config.dbFile);
                string json = ExtenderGzip.Decompress(compressed);
                db = JsonConvert.DeserializeObject<Dictionary<int,ExtendedEntry>>(json);
            }
            else {
                db = new Dictionary<int, ExtendedEntry>();
            }

            new Thread(new ThreadStart(ScheduledSave)).Start();
        }

        private static void SaveDB() {
            while(dbLock) {}

            dbLock = true;
            string json = JsonConvert.SerializeObject(db, Formatting.None);
            var compressed = ExtenderGzip.Compress(json);
            File.WriteAllBytes(config.dbFile, compressed);
            dbLock = false;
        }

        private static void ScheduledSave() {
            while(true) {
                Thread.Sleep(60000);
                SaveDB();
            }
        }

        private static string getUrlFromCount(int count) {
            string url = "";

            for(int i = 0; i < count; i++) {
                url += config.magicWord;
            }

            return url;
        }

        private static int getCountFromUrl(string url) {
            return Regex.Matches(url, config.magicWord).Count;
        }

        private static int generateValidCount() {
            try {
                int count = new Random().Next(1,1000);
                foreach(int i in db.Keys) {
                    if(i == count) return generateValidCount();
                }
                return count;
            }
            catch (System.StackOverflowException) { // detect infinite recursion if we keep failing to find a valid URL
                throw new OutOfURLsException("Y'all used this so much that we ran out of extended URLs. Tell me ASAP");
            }
            
        }

        public static string Add(string url) {
            int count;
            int alreadyExtended = -1;

            foreach(int i in db.Keys) {
                if(db[i].url == url) alreadyExtended = i;
            }

            if(alreadyExtended != -1) {
                return getUrlFromCount(alreadyExtended);
            }
            else {
                count = generateValidCount();  
            }

            db.Add(count, new ExtendedEntry(url));

            new Thread(new ThreadStart(SaveDB)).Start();
            return getUrlFromCount(count);
        }

        public static string Dereferance(string url) {
            int count = getCountFromUrl(url);
            db[count].hits++;
            return db[count].url;
        }

        public static int GetHits(string url) {
            foreach(int i in db.Keys) {
                if(db[i].url == url) {
                    return db[i].hits;
                }
            }
            throw new KeyNotFoundException("The given URL has not been extended");
        }
    }

    public class OutOfURLsException : Exception {
        public OutOfURLsException() {}

        public OutOfURLsException(string message) : base(message) {}

        public OutOfURLsException(string message, Exception inner) : base(message, inner) {}
    }
}