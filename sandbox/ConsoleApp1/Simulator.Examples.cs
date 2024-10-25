using R3;

public partial class Simulator
{
    public static async Task Sample1()
    {
        var sequencesStr = """
            -- -- -- 20 -- 40 -- 60 -- -->
            -- 01 -- 02 -- 03 -- -- -- -->
            -- -- -- -- 00 -- 00 -- 00 -->
            """;
        var sampleGen = new Simulator();
        await sampleGen.Run(sequencesStr, sequences => Observable.Amb(sequences));

        // Number:       1  2  3  4  5  6  7  8  9 10
        // Sequence 1:  -- -- -- 20 -- 40 -- 60 -- -->
        // Sequence 2:  -- 01 -- 02 -- 03 -- -- -- -->
        // Sequence 3:  -- -- -- -- 00 -- 00 -- 00 -->
        // 
        // Results:     -- 01 -- 02 -- 03 -- -- -- -->
    }

    public static async Task Sample2()
    {
        var sequencesStr = """
            ------20--40--60------>
            --01--02--03---------->
            --------00--00----00-->
            """;
        var sampleGen = new Simulator(cfg =>
        {
            cfg.InputItemsSeparatedBySpace = false;
            cfg.OutputMode = SampleGeneratorOutputMode.List;
        });
        await sampleGen.Run(sequencesStr, sequences =>
            Observable.CombineLatest(sequences).Select(strs => $"[{string.Join(",", strs)}]")
        );

        // Number:       1  2  3  4  5  6  7  8  9 10 11
        // Sequence 1:  -- -- -- 20 -- 40 -- 60 -- -- -->
        // Sequence 2:  -- 01 -- 02 -- 03 -- -- -- -- -->
        // Sequence 3:  -- -- -- -- 00 -- 00 -- -- 00 -->
        // 
        // Results:
        //   1: --
        //   2: --
        //   3: --
        //   4: --
        //   5: [20,02,00]
        //   6: [40,02,00], [40,03,00]
        //   7: [40,03,00]
        //   8: [60,03,00]
        //   9: --
        //  10: [60,03,00]
        //  11: --
    }

    public static async Task Sample3()
    {
        var sequencesStr = """
            -- -- -- 20 -- 40 -- 60 -- -->
            -- 01 -- 02 -- 03 -- -- -- -->
            -- -- -- -- 00 -- 00 -- 00 -->
            """;
        var sampleGen = new Simulator(cfg =>
        {
            cfg.AdditionalLegend = "_";
            cfg.GlobalPrefix = "** ";
            cfg.PrintArrowTipOnStart = true;
            cfg.RunInterval = TimeSpan.FromSeconds(0.1);
        });
        await sampleGen.Run(sequencesStr, sequences =>
            Observable.Merge(sequences)
        );

        // ** Number:       1  2  3  4  5  6  7  8  9 10
        // ** Sequence 1: > -- -- -- 20 -- 40 -- 60 -- -->
        // ** Sequence 2: > -- 01 -- 02 -- 03 -- -- -- -->
        // ** Sequence 3: > -- -- -- -- 00 -- 00 -- 00 -->
        // **
        // ** Results:    > -- 01 -- 20 00 40 00 60 00 -->
        // ** _           >          02    03            >
    }
}
