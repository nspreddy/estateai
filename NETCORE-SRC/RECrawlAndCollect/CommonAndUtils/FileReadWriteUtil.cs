using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace CommonAndUtils
{
    public class FileReadWriteUtil
    {
        public static void WriteToFile(string dir, string outputFile, string content)
        {
            try
            {
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var completeFilePath = Path.Combine(dir, outputFile);
                WriteToFile(completeFilePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to Write to file {outputFile}, exception: {ex.Message}");
            }
        }

        public static void WriteToFile(string completeFilePath, string content)
        {
            try
            {
                using (StreamWriter file = File.CreateText(completeFilePath))
                {
                    file.WriteLine(content);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Failed to Write to file {completeFilePath}, exception: {ex.Message}");
            }
        }
    }
}
