using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace File_Exchange_Manager
{
    class RegexpParsing
    {
        public static string DataInserting(string initStr, DateTime dt)
        {
            string pattern = @"%\w+%";
            //string pattern = @"%yyyy%";
            Regex regex = new Regex(pattern);
            
            foreach (Match match in regex.Matches(initStr))
            {
                
                char[] trimmed = {'%'};
                string founded = match.Value.Trim(trimmed);

                if (founded == "days")
                {                    
                    DateTime startOfYear = new DateTime(dt.Year, 1, 1);
                    double days = Math.Round((dt - startOfYear).TotalDays);
                    initStr = initStr.Replace("%days%", days.ToString());
                }
                else
                {
                    try
                    {
                        string replacement = dt.ToString(founded);
                        initStr = initStr.Replace("%" + founded + "%", replacement);
                    }
                    catch (Exception)
                    {
                        //ToDo: отлавливание исключения
                        //честно говоря, не знаю что должно выполняться в этом случае
                    }
                }
            }

            return initStr.Replace("%", ""); ;
        }

        public static string UTCDataInserting(string initStr, DateTime dt)
        {
            return RegexpParsing.DataInserting(initStr, dt.ToUniversalTime());
        }

        public static bool FilenameVerification(string filename, string mask)
        {
            List<string> matches = new List<string>();
            Regex regex = FindFilesPatternToRegex.Convert(mask);
            bool result = regex.IsMatch(filename);
            return result;
        }

        private static class FindFilesPatternToRegex
        {
            private static Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
            private static Regex IllegalCharactersRegex = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);
            private static Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
            private static string NonDotCharacters = @"[^.]*";

            public static Regex Convert(string pattern)
            {
                if (pattern == null)
                {
                    throw new ArgumentNullException();
                }
                pattern = pattern.Trim();
                if (pattern.Length == 0)
                {
                    throw new ArgumentException("Pattern is empty.");
                }
                if (IllegalCharactersRegex.IsMatch(pattern))
                {
                    throw new ArgumentException("Pattern contains illegal characters.");
                }
                bool hasExtension = CatchExtentionRegex.IsMatch(pattern);
                bool matchExact = false;
                if (HasQuestionMarkRegEx.IsMatch(pattern))
                {
                    matchExact = true;
                }
                else if (hasExtension)
                {
                    matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
                }
                string regexString = Regex.Escape(pattern);
                regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
                regexString = Regex.Replace(regexString, @"\\\?", ".");
                if (!matchExact && hasExtension)
                {
                    regexString += NonDotCharacters;
                }
                regexString += "$";
                Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return regex;
            }
        }
    }
}
