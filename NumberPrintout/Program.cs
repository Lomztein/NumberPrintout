using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        static Dictionary<long, string> prefixes = new Dictionary<long, string> ();

        static void Main(string [ ] args) {
            prefixes.Add (2, "hundred");
            prefixes.Add (3, "thousand");
            prefixes.Add (6, "million");
            prefixes.Add (9, "billion");
            prefixes.Add (12, "trillion");
            prefixes.Add (15, "quadrillion");
            prefixes.Add (18, "quintillion"); // It litteraly cannot go above this due to long.MaxValue being about 9 quintillion.
            prefixes.Add (21, "sextillion");
            prefixes.Add (24, "septillion");

            while (true) {
                string input = Console.ReadLine ();
                long number;
                if (long.TryParse (input, out number)) {
                    Console.WriteLine (NumberToEnglish (number, "", false));
                } else {
                    Console.WriteLine ("Failed to parse input. Maximum is about 9 quintillion, or you might have a non-number character.");
                }
            }
        }

        public static string NumberToEnglish(long number, string suffix, bool fromPrevious) {
            return NumberToEnglish (number.ToString (), suffix, fromPrevious);
        }

        public static string NumberToEnglish(string input, string suffix, bool fromPrevious) {
            List<long> atMagnitude = new List<long> ();
            long number;
            string result = "";

            if (long.TryParse (input, out number)) {
                input = number.ToString ();
                foreach (Char c in input) {
                    if (c == '-')
                        continue;
                    atMagnitude.Add (long.Parse (c + ""));
                }
                number = Math.Abs (number);
                atMagnitude.Reverse (); // Reverse so they actually fit the name.
                // atMagnitudes is filled, now to parse them..

                long highestMagnitudeNumber = 0;
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

        public static string GetMagnitudePrefix(long magnitude, out long magnitudeNumber) {
            string result = "";
            magnitudeNumber = 0;

            foreach (KeyValuePair<long, string> pair in prefixes) {
                if (pair.Key <= magnitude)
                    magnitudeNumber = (long)Math.Pow (10, pair.Key);

                if (pair.Key == magnitude) {
                    result = pair.Value;
                }
            }

            return result;
        }

        public static string GetSubTwenty(long ones, long tens) {
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

        public static string TenthToEnglish(long tenth) {
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

        public static string OnethToEnglish(long oneth) {
            switch (oneth) {
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
