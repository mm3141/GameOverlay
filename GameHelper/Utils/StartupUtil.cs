// <copyright file="StartupUtil.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Utils
{
    using System;
    using System.Linq;

    public class StartupUtil
    {
        public static string GenerateWindowTitle()
        {
            //more common letters!
            const string characters = "qwertyuiopasdfghjklzxcvbnm" + "eioadfc";
            var random = new Random();

            char GetRandomCharacter()
            {
                return characters[random.Next(0, characters.Length)];
            }

            string GetWord()
            {
                return char.ToUpperInvariant(GetRandomCharacter()) +
                       new string(Enumerable.Range(0, random.Next(5, 10))
                                            .Select(_ => GetRandomCharacter())
                                            .ToArray());
            }

            return string.Join(' ', Enumerable.Range(0, random.Next(1, 4)).Select(_ => GetWord()));
        }
    }
}
