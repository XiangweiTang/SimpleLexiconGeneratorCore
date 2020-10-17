using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SimpleLexiconGeneratorCore
{
    static class IO
    {
        public static IEnumerable<string> ReadEmbed(string path)
        {
            Assembly asmb = Assembly.GetExecutingAssembly();
            using (StreamReader sr = new StreamReader(asmb.GetManifestResourceStream(path)))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    yield return line;
            }
        }
    }
}
