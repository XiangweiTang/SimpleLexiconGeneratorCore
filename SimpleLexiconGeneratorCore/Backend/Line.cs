using System.Collections.Generic;

namespace SimpleLexiconGeneratorCore
{
    abstract class Line
    {
        public Line() { }
        public Line(string line)
        {
            var split = line.Split('\t');
            SetLine(split);
        }

        public string Output()
        {
            return string.Join("\t", GetLine());
        }

        abstract protected IEnumerable<string> GetLine();
        abstract protected void SetLine(string[] split);
    }

    class DataLine : Line
    {
        public string WavPath { get; set; }
        public string Accent { get; set; }
        public string HighGerman { get; set; }
        public string Context { get; set; }
        public string Lexicon { get; set; } = "";
        public DataLine() { }
        public DataLine(string line) : base(line)
        {
        }

        protected override IEnumerable<string> GetLine()
        {
            yield return WavPath;
            yield return Accent;
            yield return HighGerman;
            yield return Context;
            yield return Lexicon;
        }

        protected override void SetLine(string[] split)
        {
            WavPath = split[0];
            Accent = split[1];
            HighGerman = split[2];
            Context = split[3];
            if (split.Length > 4)
                Lexicon = split[4];
        }
    }

    class PhoneticLine : Line
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public string XSampaSymbol { get; set; }
        public string IpaSymbol { get; set; }
        public string ColumnId { get; set; }
        public PhoneticLine(string line) : base(line) { }
        protected override IEnumerable<string> GetLine()
        {
            yield return Category;
            yield return Type;
            yield return XSampaSymbol;
            yield return IpaSymbol;
            yield return ColumnId;
        }

        protected override void SetLine(string[] split)
        {
            Category = split[0];
            Type = split[1];
            XSampaSymbol = split[2];
            IpaSymbol = split[3];
            ColumnId = split[4];
        }
    }
}
