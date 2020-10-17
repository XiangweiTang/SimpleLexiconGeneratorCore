using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace SimpleLexiconGeneratorCore
{
    class Backend
    {
        DataLine[] DataArray = new DataLine[0];
        private HashSet<string> ValidTokenSet = new HashSet<string>();
        private SoundPlayer Player = new SoundPlayer();
        public int CurrentIndex { get; private set; } = 0;
        public int CurrentExternalIndex => CurrentIndex + 1;
        public DataLine CurrentLine => CurrentIndex < DataArray.Length && CurrentIndex >= 0 ? DataArray[CurrentIndex] : null;
        public string Column1String { get; private set; }
        public string Column2String { get; private set; }
        public string Column3String { get; private set; }
        public string Column4String { get; private set; }
        public string[] AudioNames => DataArray.Select((x, y) => $"{y + 1}   {x.WavPath}").ToArray();
        public string InputTextFilePath { get; set; }
        public string InputAudioFolderPath { get; set; }
        public Backend()
        {
        }
        public void Init()
        {
            DataArray = SafeRead(InputTextFilePath).Select(x => new DataLine(x)).ToArray();
            CurrentIndex = 0;
            SetLabels();
        }

        private IEnumerable<string> SafeRead(string filePath)
        {
            if (File.Exists(filePath))
                return File.ReadLines(filePath);
            return new string[0];
        }
        private void SetLabels()
        {
            string path = "SimpleLexiconGeneratorCore.Internal.Phonetics.txt";
            var list = IO.ReadEmbed(path).Select(x => new PhoneticLine(x)).ToArray();
            ValidTokenSet = list.Select(x => x.XSampaSymbol).ToHashSet();
            var groups = list.ToLookup(x => x.ColumnId);
            Column1String = string.Join("\r\n", LabelOutput(groups["1"]));
            Column2String = string.Join("\r\n", LabelOutput(groups["2"]));
            Column3String = string.Join("\r\n", LabelOutput(groups["3"]));
            Column4String = string.Join("\r\n", LabelOutput(groups["4"]));
        }

        private IEnumerable<string> LabelOutput(IEnumerable<PhoneticLine> list)
        {
            var groups = list.GroupBy(x => x.Type);
            yield return "XSampa\t IPA";
            foreach (var group in groups)
            {
                yield return group.Key;
                foreach (var line in group)
                    yield return $"{line.XSampaSymbol}\t {line.IpaSymbol}";
            }
        }
        public void IndexIncrease()
        {
            if (CurrentIndex < DataArray.Length - 1)
                CurrentIndex++;
        }

        public void IndexDecrease()
        {
            if (CurrentIndex > 0)
                CurrentIndex--;
        }

        public void SetIndex(int i)
        {
            if (i < 0)
                CurrentIndex = 0;
            else if (i >= DataArray.Length)
                CurrentIndex = DataArray.Length - 1;
            else
                CurrentIndex = i;
        }
        public void Play()
        {
            string wavPath = Path.Combine(InputAudioFolderPath, CurrentLine.WavPath);
            Sanity.Requires(File.Exists(wavPath), $"Missing file {wavPath}");
            using (FileStream fs = new FileStream(wavPath, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    Player.Stream = fs;
                    Player.Play();
                }
                catch
                {
                    MessageBox.Show("File error.");
                }
            }
        }

        public void Stop()
        {
            Player.Stop();
        }

        public void SaveCurrentData(string accent, string highGerman, string context, string lexicon)
        {
            Sanity.Requires(DataArray.Length > 0, "The data file is empty!");
            var line = new DataLine
            {
                WavPath = CleanupData(CurrentLine.WavPath),
                Accent = CleanupData(accent),
                HighGerman = CleanupData(highGerman),
                Context = CleanupData(context),
                Lexicon = ValidLexicon(lexicon)
            };

            DataArray[CurrentIndex] = line;
        }
        Regex SpaceReg = new Regex("\\s+", RegexOptions.Compiled);
        private string CleanupData(string inputString)
        {
            return SpaceReg.Replace(inputString, " ").Trim();
        }
        public void SaveAllData(string saveFilePath)
        {
            var list = DataArray.Select(x => x.Output());
            File.WriteAllLines(saveFilePath, list);
        }

        public string ValidLexicon(string i)
        {
            StringBuilder tokenSb = new StringBuilder();
            StringBuilder sentSb = new StringBuilder();
            bool isPrimStress = false;
            foreach (char c in i)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (tokenSb.Length > 0)
                    {
                        string token = tokenSb.ToString();
                        Sanity.Requires(ValidToken(token), $"Invalid phone <{token}>, in work item [{CurrentExternalIndex}].");
                        if (sentSb.Length > 0 && !isPrimStress)
                            sentSb.Append(' ');
                        sentSb.Append(token);
                        isPrimStress = token == "\"";
                        tokenSb.Clear();
                    }
                    continue;
                }
                tokenSb.Append(c);
            }
            if (tokenSb.Length > 0)
            {
                string last = tokenSb.ToString();
                Sanity.Requires(ValidToken(last), $"Invalid phone <{last}>, in work item [{CurrentExternalIndex}].");
                if (sentSb.Length > 0 && !isPrimStress)
                    sentSb.Append(' ');
                sentSb.Append(last);
            }

            return sentSb.ToString();
        }

        private bool ValidToken(string token)
        {
            if (ValidTokenSet.Contains(token))
                return true;
            if (token[0] != '"')
                return false;
            return ValidTokenSet.Contains(token.Substring(1));
        }

        public string IncompleteTasksInfo()
        {
            var incomplete = DataArray.Select((x, y) => new { x.Lexicon, y })
                .Where(x => string.IsNullOrWhiteSpace(x.Lexicon))
                .Take(10)
                .Select(x => x.y)
                .ToArray();
            string message;
            if (incomplete.Length == 0)
            {
                message = "All tasks are done!";
            }
            else if (incomplete.Length == 10)
            {
                message = $"At least 10 tasks are not done yet:\r\n{string.Join(",", incomplete.Select(x => $"[{x + 1}]"))}";
            }
            else
            {
                message = $"{incomplete.Length} tasks are not done yet:\r\n{string.Join(",", incomplete.Select(x => $"[{x + 1}]"))}";
            }
            return message;
        }
    }
}
