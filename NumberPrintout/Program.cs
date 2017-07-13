using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

namespace NumberPrintout {
    class Program {

        // AUTHOR - Marcus "Lomztein" Jensen, copyright 2017.

        /* I think I understand my own code enough to explain how it works.
         * To put it shortly, the input is first split into orders of magnitude,
         * where every magnitude is saved as a single long in a list. This list is
         * gone through, and the highest raw magnitude prefix is found (see dictionary additions).
         * Once the highest prefix is found, it splits the number into pre and post prefix. 
         * These are then looped back into the system, creating a recursive loop, effectively
         * splitting the number up into prefixes which are then individually torn down into
         * the smallest prefix (hundred). Once this is done, they are named. Everything
         * is being stitched together on the fly. The stitching part is a mess, don't look at it, please.
        */

        static Dictionary<int, string> prefixes = new Dictionary<int, string> ();

        static void Main(string [ ] args) {
            prefixes.Add (2, "hundred");
            prefixes.Add (3, "thousand");
            prefixes.Add (6, "million");
            prefixes.Add (9, "billion");
            prefixes.Add (12, "trillion");
            prefixes.Add (15, "quadrillion");
            prefixes.Add (18, "quintillion");
            prefixes.Add (21, "sextillion");
            prefixes.Add (24, "septillion");
            prefixes.Add (27, "octillion");
            prefixes.Add (30, "nonillion");
            prefixes.Add (33, "decillion");
            prefixes.Add (36, "undecillion");
            prefixes.Add (39, "duodecillion");
            prefixes.Add (42, "tredecillion");
            prefixes.Add (100, "googol");
            // Next is a googolplex, which is a ten followed by a googol zeroes, and I'm not gonna type that in.

            bool running = true;
            BigInteger number = 0;
            while (running) {
                string input = Console.ReadLine ();

                if (BigInteger.TryParse (input, out number)) { // A number is input
                    Console.WriteLine (NumberToEnglish (number.ToString (), "", false));

                } else if (input.Length >= 5 && input.Substring (0, 5) == "range") { // A range is input
                    string [ ] split = input.Substring (6).Split (' ');
                    BigInteger start;
                    BigInteger stop;

                    if (split.Length > 1 && BigInteger.TryParse (split [ 0 ], out start) && BigInteger.TryParse (split [ 1 ], out stop)) {
                        for (number = start; number <= stop; number++) {
                            Console.WriteLine (NumberToEnglish (number.ToString (), "", false));
                        }
                    } else {
                        Console.WriteLine ("Failed to parse start and/or stop.");
                    }
                } else if (File.Exists (input)) { // A file is input
                    Console.WriteLine (NumberToEnglish (File.ReadAllText (input), "", false));

                } else {
                    Console.WriteLine ("Failed to do anything, please try again.");
                }
            }
        }

        public static string NumberToEnglish(string input, string suffix, bool fromPrevious) {
            List<byte> atMagnitude = new List<byte> ();
            BigInteger number;
            string result = "";

            if (BigInteger.TryParse (input, out number)) {
                input = number.ToString ();

                if (!fromPrevious && input == "0") // Good programming is overrated anyways.
                    return "zero";

                foreach (Char c in input) {
                    if (c == '-')
                        continue;
                    atMagnitude.Add (byte.Parse (c + ""));
                }
                number = BigInteger.Abs (number);

                atMagnitude.Reverse (); // Reverse so they actually fit the name.
                // atMagnitudes is filled, now to parse them..

                BigInteger highestMagnitudeNumber = new BigInteger(0);
                string magnitudePrefix = "";
                for (int i = 0; i < atMagnitude.Count; i++) {
                    string newPrefix = GetMagnitudePrefix (i, out highestMagnitudeNumber);
                    if (newPrefix != "")
                        magnitudePrefix = newPrefix;
                }

                // Highest magnitude has been found.
                bool addedSubTwenty = false;
                if (highestMagnitudeNumber == 0) {
                    // Is below any magnitude, AE below 100.
                    if (atMagnitude.Count > 1) {
                        if (atMagnitude [ 1 ] == 1) {
                            result += GetSubTwenty (atMagnitude [ 0 ], atMagnitude [ 1 ]);
                            addedSubTwenty = true;
                        } else {
                            result += TenthToEnglish (atMagnitude [ 1 ]) + "ty ";
                        }
                    }
                    if (atMagnitude.Count > 0) {
                        if (!addedSubTwenty)
                            result += OnethToEnglish (atMagnitude [ 0 ]);
                    }
                    result += " " + suffix;
                } else {
                    // Is above a magnitude AE above or equal to 100.
                    string pastMagnitude = number.ToString ().Substring (0, number.ToString ().Length - (highestMagnitudeNumber.ToString ().Length - 1));
                    string postMagnitude = number.ToString ().Substring (pastMagnitude.Length);
                    result += NumberToEnglish (pastMagnitude, magnitudePrefix, true) + " " + NumberToEnglish (postMagnitude, "", true) + suffix;
                }

            } else {
                Console.WriteLine ("Failed to parse input.");
            }

            return result;
        }

        public static string GetMagnitudePrefix(int magnitude, out BigInteger magnitudeNumber) {
            string result = "";
            magnitudeNumber = new BigInteger (0);

            foreach (KeyValuePair<int, string> pair in prefixes) {
                if (pair.Key <= magnitude)
                    magnitudeNumber = BigInteger.Pow (10, (int)pair.Key);

                if (pair.Key == magnitude) {
                    result = pair.Value;
                }
            }

            return result;
        }

        public static string GetSubTwenty(byte ones, byte tens) {
            if (tens > 0) {
                switch (ones) {
                    case 0:
                        return "ten";
                    case 1:
                        return "eleven";
                    case 2:
                        return "twelfth";
                    default:
                        return TenthToEnglish (ones) + "teen";
                }
            } else {
                return "";
            }
        }

        public static string TenthToEnglish(byte tenth) {
            switch (tenth) {
                case 2:
                    return "twen";
                case 3:
                    return "thir";
                case 5:
                    return "fif";
                case 8:
                    return "eigh";
                default:
                    return OnethToEnglish (tenth);
            }
        }

        public static string OnethToEnglish(byte ones) {
            switch (ones) {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";

                default:
                    return "";
            }
        }
    }
}
