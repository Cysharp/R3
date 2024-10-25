using R3;

/// <summary>
/// Class that runs simulation to make samples for documentation
/// </summary>
public partial class Simulator
{
    SimulatorConfig _cfg;

    public Simulator(SimulatorConfig? cfg = null)
    {
        _cfg = cfg ?? SimulatorConfig.Default;

        if(_cfg.EmptyItem.Length != _cfg.TerminatorItem.Length)
            throw new Exception("Empty and Terminator must have the same length");
    }

    public Simulator(Action<SimulatorConfig> configure) : this(SimulatorConfig.Create(configure))
    {
    }

    public SimulatorConfig Config => _cfg;

    public delegate Observable<string> ObservableOperationDelegate(Observable<string>[] sequences);
    public delegate Task<Observable<string>> ObservableOperationAsyncDelegate(Observable<string>[] sequences);

    public Task<string[][]> Run(string sequence, ObservableOperationDelegate operation)
    {
        return Run(sequence, sequences => Task.FromResult(operation(sequences)));
    }

    /// <summary>
    /// <para>
    ///   Runs custom observable operation on the provided sequences.
    /// </para>
    /// <para>Example of usage (from <see cref="Simulator.Sample1"/>):</para>
    /// <example>
    ///   <code>
    ///   var sequencesStr = """
    ///       -- -- -- 20 -- 40 -- 60 -- -->
    ///       -- 01 -- 02 -- 03 -- -- -- -->
    ///       -- -- -- -- 00 -- 00 -- 00 -->
    ///       """;
    ///   var sampleGen = new SampleGenerator();
    ///   await sampleGen.Run(sequencesStr, sequences => Observable.Amb(sequences));
    ///   </code>
    /// </example>
    /// </summary>
    /// <param name="sequence">String representing the sequence</param>
    /// <param name="operation"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string[][]> Run(string sequence, ObservableOperationAsyncDelegate operation)
    {
        using var disposable = new CompositeDisposable();

        // Remove arrow tips
        sequence = sequence.Replace(_cfg.ArrowTip, string.Empty);

        // Split sequence string into array of arrays of items
        var sequencesOfItems = sequence
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var items = _cfg.InputItemsSeparatedBySpace ?
                    line.Split(' ') :
                    line.Chunk(2).Select(c => new string(c));
                return items.ToArray();
            })
            .ToArray();

        var sequenceLength = sequencesOfItems[0].Length;
        var resultingSequence = Enumerable.Repeat(0, sequenceLength).Select(_ => new List<string>()).ToList();

        var intervalStepCounter = 0;

        // action that prints item, accepts step id, id of result within step and string value
        var printItem = new Action<int, int, string>((_, _, _) => { });
        // action that will be called at the end of run
        var finaliseStr = new Action(() => { });

        ICollection<int> finishedIds = sequencesOfItems.Length > 40 ? new HashSet<int>() : new List<int>();

        if (_cfg.PrintResultTo != null)
        {
            int numberAdditive = _cfg.StartNumbersAtZero ? 0 : 1;
            var sequenceLegendFormat = sequencesOfItems.Length > 1 ? _cfg.SequenceLegendFormat : _cfg.SingleSequenceLegend;

            // width of single item on timeline
            int itemWidth = _cfg.EmptyItem.Length + (_cfg.AddSpacing ? 1 : 0);
            // width of legend column is length widest legend string
            int legendWidth = 0;
            if (_cfg.AddLegend)
            {
                legendWidth = new[] {
                    _cfg.NumberLegend,
                    _cfg.ResultsLegend,
                    _cfg.AdditionalLegend,
                    _cfg.SingleSequenceLegend,
                    string.Format(sequenceLegendFormat, sequencesOfItems.Length - numberAdditive)
                }
                .Select(s => s.Length)
                .Max();
            }

            var writer = _cfg.PrintResultTo;
            // print numbers row
            if (_cfg.AddNumbers)
            {
                writer.Write(_cfg.GlobalPrefix);
                if (_cfg.AddLegend)
                {
                    writer.Write(_cfg.NumberLegend.PadRight(legendWidth));
                }
                for (var i = 0; i < sequenceLength; i++)
                {
                    writer.Write((i + numberAdditive).ToString().PadLeft(itemWidth));
                }
                writer.WriteLine();
            }

            // print initial sequences
            for(var i = 0; i < sequencesOfItems.Length; i++)
            {
                writer.Write(_cfg.GlobalPrefix);
                if (_cfg.AddLegend)
                {
                    writer.Write(string.Format(sequenceLegendFormat, i + numberAdditive).PadRight(legendWidth));
                }
                if (_cfg.PrintArrowTipOnStart)
                {
                    writer.Write(_cfg.ArrowTip);
                }
                foreach (var item in sequencesOfItems[i])
                {
                    writer.Write(item.PadLeft(itemWidth));
                }
                writer.WriteLine(_cfg.ArrowTip);
            }

            // print empty line before results
            if (_cfg.EmptyLineBeforeResults)
            {
                writer.WriteLine(_cfg.GlobalPrefix);
            }

            // print results legend
            writer.Write(_cfg.GlobalPrefix);
            if (_cfg.OutputMode == SampleGeneratorOutputMode.List)
            {
                writer.Write(_cfg.ResultsLegend);
            }
            if (_cfg.OutputMode == SampleGeneratorOutputMode.Timeline)
            {
                writer.Write(_cfg.ResultsLegend.PadRight(legendWidth));
                if (_cfg.PrintArrowTipOnStart)
                {
                    writer.Write(_cfg.ArrowTip);
                }
            }

            // setup printItem delegate
            printItem = (id, idxWithinItem, str) =>
            {
                if (_cfg.OutputMode == SampleGeneratorOutputMode.List && idxWithinItem == 0)
                {
                    writer.WriteLine();
                    writer.Write(_cfg.GlobalPrefix);
                    writer.Write($"{(id + numberAdditive),3}: {str}");
                }
                if (_cfg.OutputMode == SampleGeneratorOutputMode.List && idxWithinItem > 0)
                {
                    writer.Write($", {str}");
                }
                if (_cfg.OutputMode == SampleGeneratorOutputMode.Timeline)
                {
                    writer.Write(str.PadLeft(itemWidth));
                }
            };

            if (_cfg.OutputMode == SampleGeneratorOutputMode.Timeline)
            {
                // setup finaliseStr delegate
                finaliseStr = () =>
                {
                    writer.WriteLine(_cfg.ArrowTip);
                    var maxAmountPerItem = resultingSequence.Max(s => s.Count);

                    for (var i = 1; i < maxAmountPerItem; i++)
                    {
                        writer.Write(_cfg.GlobalPrefix);
                        writer.Write(_cfg.AdditionalLegend.PadRight(legendWidth));
                        if (_cfg.PrintArrowTipOnStart)
                        {
                            writer.Write(_cfg.ArrowTip);
                        }

                        for (var j = 0; j < resultingSequence.Count; j++)
                        {
                            if (resultingSequence[j].Count > i)
                            {
                                writer.Write(resultingSequence[j][i].PadLeft(itemWidth));
                            }
                            else
                            {
                                writer.Write(new string(' ', itemWidth));
                            }
                        }
                        writer.WriteLine(_cfg.PrintArrowTipOnAdditionalLines ? _cfg.ArrowTip : string.Empty);
                    }
                };
            }
        }

        
        var sequences = sequencesOfItems.Select(_ => new Subject<string>()).ToArray();

        var locker = new object();
        var semaphore = new SemaphoreSlim(0, 1);
        var interval = Observable.Interval(_cfg.RunInterval);

        // Every tick print items that were obtained in last tick, then movint next step in sequence
        interval.Subscribe(_ =>
        {
            lock (locker)
            {
                // printing items
                if (intervalStepCounter > 0)
                {
                    var items = resultingSequence[intervalStepCounter - 1];
                    if (_cfg.OutputMode == SampleGeneratorOutputMode.Timeline && items.Count > 0)
                    {
                        items = new() { items[0] };
                    }
                    if(items.Count == 0)
                    {
                        items = new() { _cfg.EmptyItem };
                    }

                    for (var i = 0; i < items.Count; i++)
                    {
                        printItem(intervalStepCounter - 1, i, items[i]);
                    }
                }

                // Either move to next step, either finish the sequence.
                // When all sequences are finished, release semaphore to finish the run.
                for (var i = 0; i < sequencesOfItems.Length; i++)
                {
                    if (finishedIds.Contains(i))
                        continue;

                    var currentItem = _cfg.EmptyItem;
                    if (intervalStepCounter >= sequenceLength && _cfg.TerminateOnEndOfData)
                        currentItem = _cfg.TerminatorItem;
                    if (intervalStepCounter < sequenceLength)
                        currentItem = sequencesOfItems[i][intervalStepCounter];

                    if (currentItem == _cfg.TerminatorItem)
                    {
                        finishedIds.Add(i);
                        sequences[i].OnCompleted();
                        if (finishedIds.Count == sequencesOfItems.Length)
                        {
                            semaphore.Release();
                        }
                        continue;
                    }
                    if (currentItem != _cfg.EmptyItem)
                    {
                        sequences[i].OnNext(currentItem);
                    }
                }
                intervalStepCounter++;
            }
        }).AddTo(disposable);

        // Subscribe to operation. Update resulting sequence with new values.
        var subscribeTo = await operation(sequences);
        subscribeTo.Subscribe(s =>
        {
            lock (locker)
            {
                while (intervalStepCounter >= resultingSequence.Count)
                {
                    resultingSequence.Add(new List<string>());
                }
                resultingSequence[intervalStepCounter].Add(s);
            }
        }).AddTo(disposable);

        // Wait for execution to finish
        await semaphore.WaitAsync();
        // Finishing printing
        finaliseStr();

        return resultingSequence.Select(s => s.ToArray()).ToArray();
    }
}

public class SimulatorConfig
{
    public static SimulatorConfig Default => new();
    public static SimulatorConfig Create(Action<SimulatorConfig> configure)
    {
        var config = Default;
        configure(config);
        return config;
    }

    /// <summary>
    /// Prefix that will be added to each line of output
    /// </summary>
    public string GlobalPrefix { get; set; } = string.Empty;
    public string NumberLegend { get; set; } = "Number: ";
    public string ResultsLegend { get; set; } = "Results: ";
    public string AdditionalLegend { get; set; } = "Additional: ";
    public string SingleSequenceLegend { get; set; } = "Sequence: ";
    /// <summary>
    /// Format for sequence legend. Should contain one placeholder for sequence number.
    /// </summary>
    public string SequenceLegendFormat { get; set; } = "Sequence {0}: ";
    /// <summary>
    /// Value that represents empty item in sequence, so no event emission.
    /// Also length of this string sets width of item on timeline in general.
    /// Should be same length as <see cref="TerminatorItem"/>
    /// </summary>
    public string EmptyItem { get; set; } = "--";
    /// <summary>
    /// Value that represents completion of sequence. Should be same length as <see cref="EmptyItem"/>
    /// </summary>
    public string TerminatorItem { get; set; } = "|-";
    /// <summary>
    /// Arrow tip that is printed at the end of each sequence. Also input sequences string should use same arrow tip.
    /// </summary>
    public string ArrowTip { get; set; } = ">";
    /// <summary>
    /// If true, then input sequence is expected to be in space-separated format <c>-- 60 --></c>.
    /// Otherwise expected format is <c>--60--></c>.
    /// </summary>
    public bool InputItemsSeparatedBySpace { get; set; } = true;
    /// <summary>
    /// Wheter to add spacing between columns on timeline
    /// </summary>
    public bool AddSpacing { get; set; } = true;
    /// <summary>
    /// Whether to add column with text like "Results:" or "Sequence 1:" or no
    /// </summary>
    public bool AddLegend { get; set; } = true;
    /// <summary>
    /// Whether to add numbers row before timeline
    /// </summary>
    public bool AddNumbers { get; set; } = true;
    /// <summary>
    /// If <see langword="true"/>, then input sequence considered as finished when reached it's end.
    /// Otherwise sequence will continue after reaching the end of data, but will not emit events.
    /// </summary>
    public bool TerminateOnEndOfData { get; set; } = true;
    /// <summary>
    /// Whether to use 0-based (0,1,2,...) or 1-based (1,2,3,...) indexing in displayed numbers
    /// </summary>
    public bool StartNumbersAtZero { get; set; } = false;
    /// <summary>
    /// Whether to add arrow on additional lines of result if <see cref="OutputMode"/> == <see cref="SampleGeneratorOutputMode.Timeline"/>
    /// </summary>
    public bool PrintArrowTipOnAdditionalLines { get; set; } = true;
    /// <summary>
    /// If set to <see langword="true"/>, then arrow tip will be printed before first item of sequence if <see cref="OutputMode"/> == <see cref="SampleGeneratorOutputMode.Timeline"/>
    /// </summary>
    public bool PrintArrowTipOnStart { get; set; } = false;
    /// <summary>
    /// Whether to separate results and initial timelines by empty line
    /// </summary>
    public bool EmptyLineBeforeResults { get; set; } = true;
    /// <summary>
    /// Simulation interval
    /// </summary>
    public TimeSpan RunInterval { get; set; } = TimeSpan.FromSeconds(0.1);
    public SampleGeneratorOutputMode OutputMode { get; set; } = SampleGeneratorOutputMode.Timeline;
    /// <summary>
    /// Text writer where to print the result. If set to <see langword="null"/>, result will not be printed.
    /// </summary>
    public TextWriter? PrintResultTo { get; set; } = Console.Out;
}

public enum SampleGeneratorOutputMode
{
    /// <summary>
    /// Output will be printed as timeline, like
    /// <code>
    /// -- 00 -->
    /// </code>
    /// </summary>
    Timeline,
    /// <summary>
    /// Output will be printed as list, like
    /// <code>
    /// 1: --
    /// 2: 00
    /// 3: --
    /// </code>
    /// </summary>
    List
}

