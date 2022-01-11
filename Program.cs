using System.Collections;
using System.Text;

namespace TextStego;

internal static class Program
{
    public static void Main(string[] args)
    {
        var text = File.ReadAllText("3.txt");
        var message =
            "secrettext";
        var secretMaxSize = text.Split(' ').Length / 5 / 8 - 1;
        var encoded = HideMessage2(text, message, out var maxSize);
        var startEntropy = CalculateEntropy(text);
        var endEntropy = CalculateEntropy(encoded);
        Console.WriteLine($"Начальная этропия: {startEntropy}");
        Console.WriteLine($"Конечная этропия: {endEntropy}");
        Console.WriteLine($"Длина текста: {text.Length}");
        Console.WriteLine($"Длина скрываемого сообщения: {message.Length}");
        Console.WriteLine($"Длина закодированного текста: {encoded.Length}");
        Console.WriteLine($"Максимальная длина: {secretMaxSize}");
        var decoded = DecodeMessage2(encoded);
        Console.WriteLine(encoded);
        //Console.WriteLine(decoded);
    }

    private static double CalculateEntropy(string entropyString)
    {
        var characterCounts = new Dictionary<char, int>();
        foreach (var c in entropyString.ToLower().Where(c => c != ' '))
        {
            characterCounts.TryGetValue(c, out var currentCount);
            characterCounts[c] = currentCount + 1;
        }

        var characterEntropies =
            from c in characterCounts.Keys
            let frequency = (double)characterCounts[c] / entropyString.Length
            select -1 * frequency * Math.Log(frequency);

        return characterEntropies.Sum();
    }

    private static string DecodeMessage2(string encoded)
    {
        var bitArrays = new List<BitArray>(8);
        var array = new BitArray(8);
        var counter = 0;
        var i = 0;
        var t = encoded.Split(' ').ToList();
        while (t.Count != 0)
        {
            if (counter == 8)
            {
                counter = 0;
                var stop = CharToBitArray('*');
                if (Equals(array, stop))
                {
                    break;
                }

                bitArrays.Add(array);
                array = new BitArray(8);
            }

            if (t.Count > 5)
            {
                if (t.Take(5).Contains(""))
                {
                    array[counter] = true;
                    counter++;
                    t.RemoveRange(0, 6);
                }
                else
                {
                    array[counter] = false;
                    counter++;
                    t.RemoveRange(0, 5);
                }
            }
            else
            {
                if (t.Take(t.Count).ToList().IndexOf("") != t.Count - 1)
                {
                    array[counter] = true;
                }
                else
                {
                    array[counter] = false;
                }
                bitArrays.Add(array);
                break;
            }
        }

        var secretMessage =
            bitArrays.Aggregate<BitArray, string>(null, (current, t) => current + BitArrayToStr(t));
        return secretMessage;
    }

    private static string HideMessage2(string text, string message, out int maxSize)
    {
        var t = text.Split(' ').ToList();
        message += "*";
        var bitArray = new List<BitArray>(8);
        bitArray.AddRange(message.Select(CharToBitArray));
        var stop = CharToBitArray('*');
        var result = "";
        maxSize = bitArray.Count * 8;
        if (t.Count / 5 < maxSize)
        {
            Console.WriteLine($"Контенер слишком мал. Минимальная длина контейнера должна быть {maxSize}");
            return string.Empty;
        }

        foreach (var bit in bitArray.TakeWhile(array => !Equals(array, stop)).SelectMany(array => array.Cast<bool>()))
        {
            var random = new Random();
            if (bit)
            {
                var ind = random.Next(0, 4);
                t[ind] += " ";
                for (var i = 0; i < 5; i++)
                {
                    result += t[i] + " ";
                }

                t.RemoveRange(0, 5);
            }
            else
            {
                for (var i = 0; i < 5; i++)
                {
                    result += t[i] + " ";
                }

                t.RemoveRange(0, 5);
            }
        }

        t.ForEach(_ => result += _ + " ");
        return result;
    }

    private static string DecodeMessage(string encoded)
    {
        var bitArrays = new List<BitArray>(8);
        var array = new BitArray(8);
        var counter = 0;
        var i = 0;
        while (i < encoded.Length - 1)
        {
            if (counter == 8)
            {
                counter = 0;
                var stop = CharToBitArray('*');
                if (Equals(array, stop))
                {
                    break;
                }

                bitArrays.Add(array);
                array = new BitArray(8);
            }

            if (encoded[i] == ' ')
            {
                if (encoded[i + 1] == ' ')
                {
                    array[counter] = true;
                    i += 2;
                    counter++;
                    continue;
                }

                if (encoded[i + 1] != ' ')
                {
                    array[counter] = false;
                    counter++;
                    i++;
                    continue;
                }
            }

            i++;
        }

        var secretMessage =
            bitArrays.Aggregate<BitArray, string>(null, (current, t) => current + BitArrayToStr(t));
        return secretMessage;
    }

    private static string HideMessage(string text, string message, out int maxSize)
    {
        var t = text.Split(' ').ToList();
        message += "*";
        var bitArray = new List<BitArray>(8);
        bitArray.AddRange(message.Select(CharToBitArray));
        var stop = CharToBitArray('*');
        var result = "";
        maxSize = bitArray.Count * 8;
        if (t.Count < maxSize)
        {
            Console.WriteLine($"Контенер слишком мал. Минимальная длина контейнера должна быть {maxSize}");
            return string.Empty;
        }

        foreach (var bit in bitArray.TakeWhile(array => !Equals(array, stop)).SelectMany(array => array.Cast<bool>()))
        {
            if (bit)
            {
                result += t[0] + "  ";
                t.RemoveAt(0);
            }
            else
            {
                result += t[0] + " ";
                t.RemoveAt(0);
            }
        }

        t.ForEach(_ => result += _ + " ");
        return result;
    }

    private static bool Equals(BitArray source, BitArray other)
    {
        if (source.Length != other.Length)
        {
            return false;
        }

        for (var i = 0; i < source.Length; i++)
        {
            if (!source[i] == other[i])
            {
                return false;
            }
        }

        return true;
    }

    private static string BitArrayToStr(BitArray bits)
    {
        var bytes = new byte[1];
        bits.CopyTo(bytes, 0);

        var text = Encoding.ASCII.GetString(bytes);
        return text;
    }

    private static BitArray CharToBitArray(char source)
    {
        var charByte = (byte)(source);
        var charBits = new BitArray(new[] { charByte });
        return charBits;
    }
}