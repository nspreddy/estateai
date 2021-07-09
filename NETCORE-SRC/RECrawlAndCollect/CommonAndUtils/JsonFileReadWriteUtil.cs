using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace CommonAndUtils
{
    public class JsonFileReadWriteUtil
    {
        public static void WriteJsonToFile(string dir, string outputFile, string content)
        {
            try
            {
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var completeFilePath = Path.Combine(dir, outputFile);
                using (StreamWriter file = File.CreateText(completeFilePath))
                {
                    file.WriteLine(content);                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to Write to file {outputFile}, exception: {ex.Message}");
            }
        }
    }
}
