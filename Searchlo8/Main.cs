using System.Runtime.CompilerServices;

namespace Searchlo8;

class Search3 : Searchlo8
{
    public override int[] AllowableActions()
    {
        /*  button states
            0b000000 - 0 - no input
            0b000001 - 1 - l
            0b000010 - 2 - r
            0b000100 - 4 - u
            0b001000 - 8 - d
            0b010000 - 16 - z
            0b100000 - 32 - x
            0b000101 - 5 - l + u
            0b000110 - 6 - r + u
            0b001001 - 9 - l + d
            0b001010 - 10 - r + d
            0b010001 - 17 - l + z
            0b010010 - 18 - r + z
            0b010100 - 20 - u + z
            0b011000 - 24 - d + z
            0b100001 - 33 - l + x
            0b100010 - 34 - r + x
            0b100100 - 36 - u + x
            0b101000 - 40 - d + x
            0b010101 - 21 - l + u + z
            0b010110 - 22 - r + u + z
            0b011001 - 25 - l + d + z
            0b011010 - 26 - r + d + z
            0b100101 - 37 - l + u + x
            0b100110 - 38 - r + u + x
            0b101001 - 41 - l + d + x
            0b101010 - 42 - r + d + x
        */
        int[] actions = [0b000101, 0b000110];
        return actions;
    }

    static void Main(String[] args)
    {
        Search3 s = new();
        //s.CreateLevelImage(3, 2);
        List<ActionsStruct> solutions = s.Search(200, true);

        Console.WriteLine($"inputs: {s.InputsToEnglish(solutions[0].Path)}");
    }
}
