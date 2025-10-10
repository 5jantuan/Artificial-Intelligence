namespace Battle.Utils;

public static class Evaluator
{
    public static int Evaluate(int hpMax, int hpMin)
    {
        if (hpMin <= 0 && hpMax > 0) return 10000;
        if (hpMax <= 0 && hpMin > 0) return -10000;
        if (hpMax <= 0 && hpMin <= 0) return 0;
        return hpMax - hpMin;
    }
}
