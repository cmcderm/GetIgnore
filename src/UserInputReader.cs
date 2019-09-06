using System;

namespace GetIgnore
{
    // I would really love to put this into it's out NuGet package
    // But it probably exists in a much better state out on the wild
    public class UserInputReader
    {
        public bool GetConfirmation(string question, bool defaultAnswer = false){
            Console.WriteLine("{0} ({1}): ",
                question,
                defaultAnswer ? "Y/n" : "y/N"
            );
            return isYes(Console.ReadLine(), defaultAnswer);
        }

        public bool isYes(string input, bool defaultAnswer = false)
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
    }
}