namespace UrlExtender.Objects {
    public class ExtendedEntry {
        public int hits;
        public string url;

        public ExtendedEntry() {
            hits = 0;
            url = "";
        }

        public ExtendedEntry(string _url) {
            hits = 0;
            url = _url;
        }
    }
}