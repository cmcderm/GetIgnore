using System;
using System.Collections.Generic;

namespace GetIgnore
{
    // I would really love to put this into it's out NuGet package
    // But it probably exists in a much better state out on the wild
    public class UserInputReader
    {
        static public bool GetConfirmation(string question, bool defaultAnswer = true)
        {
            Console.Write("{0} ({1}): ",
                question,
                defaultAnswer ? "Y/n" : "y/N"
            );
            return isYes(Console.ReadLine(), defaultAnswer);
        }

        /// <summary>
        /// Prompt the user to choose from a list of options. '-1' represents no selection made. 
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="choices"></param>
        /// <param name="defaultAnswer"></param>
        /// <returns></returns>
        static public int EnumerateChoices(string prompt, string selectPrompt, IList<string> choices)
        {
            Console.WriteLine(prompt);
            
            for(int i = 0; i < choices.Count; i++)
            {
                Console.WriteLine($"{i+1}) {choices[i]}");
            }

            int selection;
            try
            {
                // Yes we're starting from 1, for the user's sake
                selection = GetIntInput(selectPrompt, 1, choices.Count) - 1;
            }
            catch ( ArgumentException ex )
            {
                Console.Error.WriteLine("Something went wrong getting user input: " + ex.Message);
                return -1;
            }
            return selection;
        }

        static public bool isYes(string input, bool defaultAnswer = false)
        {
            if(input != null && input.Length != 0)
            {
                if(input.ToLower()[0] == 'y')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return defaultAnswer;
            }
        }

        /// <summary>
        /// Get an integer input from user with prompt. Returns -1 if user quits pressing q.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        static public int GetIntInput(string prompt, int min, int max, bool canQuit = true)
        {
            bool retry = false;
            do{
                Console.Write($"{prompt} ({min}-{max}): ");
                try
                {
                    string input = Console.ReadLine().ToLower();
                    if(canQuit && (input == "quit" || input == "q"))
                    {
                        return -1;
                    }
                    int selection = Convert.ToInt32(input);

                    if(selection < min || selection > max)
                    {
                        Console.WriteLine("Selection {selection} is out of range!");
                        retry = true;
                    }
                    else
                    {
                        return selection;
                    }
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.Message+Environment.NewLine);
                    retry = true;
                }
            }
            while(retry);

            throw new ArgumentException("User failed to give valid input.");
        }
    }
}