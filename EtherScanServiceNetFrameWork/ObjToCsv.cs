using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class ObjToCsv
    {
        private static StreamWriter Writer;

        public static void StartContinuousWriter<T>(string filePath, IEnumerable<T> items)
        {
            FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
            Writer = new StreamWriter(fs);

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            Writer.WriteLine(string.Join(",", props.Select(p => p.Name)));
        }

        public static void ContinuousWrite<T>(IEnumerable<T> items)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            foreach (var item in items)
            {
                Writer.WriteLine(string.Join(",", props.Select(p => p.GetValue(item, null))));
            }

            Writer.Flush();
        }

        public static void Write<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(",", props.Select(p => p.GetValue(item, null))));
                }
            }
        }
    }
}
