namespace GitHubRepositoryInfo.Extensions;

public static class MyStringExtension
{
    public static IEnumerable<int> FindAllOccurrences(this string source, string searchString)
    {
        int minIndex = source.IndexOf(searchString);
        while (minIndex != -1)
        {
            yield return minIndex;
            minIndex = source.IndexOf(searchString, minIndex + searchString.Length);
        }
    }

    public static string ReturnStringBetween(this string source, int startIndex, string character)
    {
        int nextIndex = source.IndexOf(character, startIndex);

        var charArray = source.Skip(startIndex).Take(nextIndex - startIndex).ToArray();

        return new string(charArray);
    }

    public static string GetNumberBeforeIndex(this string source, int index, int length = 50)
    {
        var charArray = new List<char>();

        var startIndex = index - length > 0 ? index - length : 0;

        var excerpt = new string(source.Skip(startIndex).Take(length).ToArray());

        var noWhiteSpace = excerpt.Replace(" ", "").Reverse().ToList();

        foreach (var digit in noWhiteSpace)
        {
            if (char.IsDigit(digit))
            {
                charArray.Add(digit);
            }
            else
            {
                break;
            }
        }

        charArray.Reverse();

        return new string(charArray.ToArray());
    }

    public static string GetNumberAndMetricAfterIndex(this string source, int index, int length = 50)
    {
        var charArray = new List<char>();

        var excerpt = new string(source.Skip(index).Take(length).ToArray());

        var trimExcerpt = excerpt.TrimStart().TrimEnd();

        var isFirstSpace = false;

        foreach (var digit in trimExcerpt)
        {
            if (char.IsLetterOrDigit(digit) || char.IsPunctuation(digit))
            {
                charArray.Add(digit);
                continue;
            }

            if (!isFirstSpace && char.IsWhiteSpace(digit))
            {
                charArray.Add(digit);
                isFirstSpace = true;
            }
            else
            {
                break;
            }
        }

        return new string(charArray.ToArray());
    }
}