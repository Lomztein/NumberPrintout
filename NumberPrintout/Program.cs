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
        static Dictionary<int, string> superPrefixes = new Dictionary<int, string> ();
        static Dictionary<int, string> superSuffixes = new Dictionary<int, string> ();

        static long numbersCrunched = 0; // I suppose this is some way represents the depth of the current iteration.

        static void Main(string [ ] args) {
            PopulatePrefixes ();
            string input = args.Length > 0 ? args [ 0 ] : "";
            bool running = true;
            BigInteger number = 0;

            // Don't read this, or I fear it might be awarded the worst interaction code in human history.
            while (running) {
                if (input == "")
                    input = Console.ReadLine ();

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
                    Console.WriteLine ("Loading file..");
                    string text = File.ReadAllText (input);
                    Console.WriteLine ("File loaded: " + text);
                    Console.WriteLine (NumberToEnglish (text, "", false));

                } else {
                    Console.WriteLine ("Failed to do anything, please try again.");
                }

                Console.WriteLine ("Recursive iterations: " + numbersCrunched);
                numbersCrunched = 0;
                input = "";
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

                BigInteger highestMagnitudeNumber = new BigInteger (0);
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
                    bool addAnd = false;
                    if (atMagnitude.Count > 0 && atMagnitude [ 0 ] != 0) {
                        addAnd = true;
                    } else if (atMagnitude.Count > 1 && atMagnitude [ 1 ] != 0) {
                        addAnd = true;
                    } // These could likely be compressed, but that would be a flustercuck.

                    if (suffix == "" && addAnd && numbersCrunched != 0)
                        AddToResult (ref result, "and");

                    if (atMagnitude.Count > 1) {
                        if (atMagnitude [ 1 ] == 1) {
                            AddToResult (ref result, GetSubTwenty (atMagnitude [ 0 ], atMagnitude [ 1 ]));
                            addedSubTwenty = true;
                        } else {
                            AddToResult (ref result, TenthToEnglish (atMagnitude [ 1 ]) + "ty");
                        }
                    }
                    if (atMagnitude.Count > 0) {
                        if (!addedSubTwenty) {
                            AddToResult (ref result, OnesToEnglish (atMagnitude [ 0 ]));
                        }
                    }

                    AddToResult (ref result, suffix);
                } else {
                    // Is above a magnitude AE above or equal to 100.
                    string pastMagnitude = number.ToString ().Substring (0, number.ToString ().Length - (highestMagnitudeNumber.ToString ().Length - 1));
                    string postMagnitude = number.ToString ().Substring (pastMagnitude.Length);
                    numbersCrunched++;

                    if (suffix != "" && postMagnitude.Length != 0)
                        suffix += ",";
                    AddToResult (ref result, NumberToEnglish (pastMagnitude, magnitudePrefix, true) + NumberToEnglish (postMagnitude, "", true) + suffix);
                }

            } else {
                Console.WriteLine ("Failed to parse input.");
            }

            return result;
        }

        public static void AddToResult(ref string result, string toAdd) {
            if (toAdd == "")
                return;

            result += toAdd;
            if (result.Length > 0) {
                if (result [ result.Length - 1 ] != ' ') {
                    result += " ";
                }
            } else {
                result += " ";
            }
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
                        return "twelve";
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
                    return OnesToEnglish (tenth);
            }
        }

        public static string OnesToEnglish(byte ones) {
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

        public static void PopulatePrefixes() {
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
            // Source (Wolfram Alpha) doesn't go above magnitude 100 (googol), except for a Googolplex, so I've followed the given pattern after that. Most likely wrong, but whatever, we get 300 magnitudes!

            superSuffixes.Add (33, "decillion");
            superSuffixes.Add (63, "vigintillion");
            superSuffixes.Add (93, "trigintillion");
            superSuffixes.Add (123, "quattuorgintillion");
            superSuffixes.Add (153, "quingintillion");
            superSuffixes.Add (183, "sexgintillion");
            superSuffixes.Add (213, "septemgintillion");
            superSuffixes.Add (243, "octogintillion");
            superSuffixes.Add (273, "novemgintillion");
            superPrefixes.Add (0, "");
            superPrefixes.Add (3, "un");
            superPrefixes.Add (6, "duo");
            superPrefixes.Add (9, "tri");
            superPrefixes.Add (12, "quattuor");
            superPrefixes.Add (15, "quin");
            superPrefixes.Add (18, "sex"); // /lennyface
            superPrefixes.Add (21, "septem");
            superPrefixes.Add (24, "octo");
            superPrefixes.Add (27, "novem");
            // I could do something fancy with the code that gets these for the numbers, but I ain't gonna. They'll just be automatically generated for the prefixes dictionary.

            foreach (KeyValuePair<int, string> suffix in superSuffixes) {
                foreach (KeyValuePair<int, string> prefix in superPrefixes) {
                    prefixes.Add (prefix.Key + suffix.Key, prefix.Value + suffix.Value);
                }
            }
        }

        public void PrintMagnitudes() {
            foreach (KeyValuePair<int, string> prefix in prefixes) {
                Console.WriteLine (prefix);
            }
        }
    }
}
