using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    enum State { Start, PairRepeat, InB, InA, Dead }

    static bool CheckByReverseFSM(string word)
    {
        if (string.IsNullOrEmpty(word) || word.Length < 2)
            return false; // нужно хотя бы одна пара

        int n = word.Length;

        // pair1 = предпоследний символ, pair2 = последний символ
        char pair1 = word[n - 2];
        char pair2 = word[n - 1];

        // индекс на символ слева от уже обработанной последней пары
        int i = n - 3; // будем двигаться влево

        State state = State.PairRepeat;

        // 1) Сначала снимаем все полные пары (pair1,pair2) с конца
        while (state == State.PairRepeat)
        {
            // проверяем, есть ли ещё пара слева (т.е. требуется 2 символа)
            if (i >= 1 && word[i - 1] == pair1 && word[i] == pair2)
            {
                i -= 2; // сняли ещё одну пару
                continue;
            }
            else
            {
                // либо пар слева больше нет, либо осталось меньше 2 символов
                state = State.InB;
            }
        }

        // 2) Теперь — блок b^m (может быть длины 0..)
        if (state == State.InB)
        {
            if (i < 0)
            {
                // ничего осталось — значит слово = (pair)^k и это корректно
                return true;
            }

            char blockB = word[i]; // символ для b^m
            while (i >= 0 && word[i] == blockB)
                i--; // снимаем все символы b^m

            // после этого переходим к проверке блока a^n
            state = State.InA;
        }

        // 3) Блок a^n — всё оставшееся (может быть пустым), но ВСЕ оставшиеся символы
        //    должны быть одинаковы (один и тот же символ blockA).
        if (state == State.InA)
        {
            if (i < 0)
            {
                // ничего не осталось — ok (a^n пустой)
                return true;
            }

            char blockA = word[i];
            while (i >= 0 && word[i] == blockA)
                i--;

            // после снятия всех blockA, если i == -1 — всё хорошо.
            return i < 0;
        }

        return false;
    }

    static void Main()
    {
        string path = "input.txt";
        if (!File.Exists(path))
        {
            Console.WriteLine($"File not found: {path}");
            return;
        }

        string text = File.ReadAllText(path);
        string[] words = text.Split(new[] { ' ', ',', '.', '!', '?', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        var matches = new List<string>();
        foreach (var w in words)
        {
            // при желании можно нормализовать регистр: w = w.ToLowerInvariant();
            if (CheckByReverseFSM(w))
                matches.Add(w);
        }

        Console.WriteLine($"Found {matches.Count} words:");
        foreach (var m in matches) Console.WriteLine(" - " + m);
    }
}
