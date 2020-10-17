using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLexiconGeneratorCore
{
    class Storage
    {
        public string CurrentTextFile { get; private set; } = "";
        public string CurrentAudioFolder { get; private set; } = "";
        private string[] SavingItems = new string[0];
        private const string BROKEN_MESSAGE = "Saving file broken, please reset values.";
        const int VALUE_COUNT = 2;
        public Storage()
        {
        }
        private IEnumerable<byte> ValueToBytes(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] header = BitConverter.GetBytes(bytes.Length);
            return header.Concat(bytes);
        }
        private byte[] ValuesToBytes()
        {
            return SavingItems.SelectMany(x => ValueToBytes(x)).ToArray();
        }
        public void Save(string path)
        {
            File.WriteAllBytes(path, ValuesToBytes());
        }
        private IEnumerable<string> BytesToValue(byte[] bytes, int index)
        {
            if (index < bytes.Length)
            {
                Sanity.Requires(index + 4 <= bytes.Length, BROKEN_MESSAGE);
                int i = BitConverter.ToInt32(bytes, index);
                Sanity.Requires(index + 4 + i <= bytes.Length, BROKEN_MESSAGE);
                string value = Encoding.UTF8.GetString(bytes, index + 4, i);
                yield return value;
                foreach (string s in BytesToValue(bytes, index + 4 + i))
                    yield return s;
            }
        }
        private void GetValues(IEnumerable<string> list)
        {
            SavingItems = list.ToArray();
            Sanity.Requires(SavingItems.Length >= VALUE_COUNT, BROKEN_MESSAGE);
            CurrentTextFile = SavingItems[0];
            CurrentAudioFolder = SavingItems[1];
        }
        public void Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var list = BytesToValue(bytes, 0);
            GetValues(list);
        }
        public void OverrideValues(params string[] values)
        {
            Sanity.Requires(values.Length == VALUE_COUNT, $"{VALUE_COUNT} items are required.");
            SavingItems = values;
            CurrentTextFile = values[0];
            CurrentAudioFolder = values[1];
        }
    }
}
