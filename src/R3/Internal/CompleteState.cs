namespace R3.Internal;

// used in Subject.
internal struct CompleteState
{
    internal enum ResultStatus
    {
        Done,
        AlreadySuccess,
        AlreadyFailed
    }

    const int NotCompleted = 0;
    const int CompletedSuccess = 1;
    const int CompletedFailure = 2;
    const int Disposed = 3;

    int completeState;
    Exception? error;

    public ResultStatus TrySetResult(Result result)
    {
        int field;
        if (result.IsSuccess)
        {
            field = Interlocked.CompareExchange(ref completeState, CompletedSuccess, NotCompleted); // try set success
        }
        else
        {
            field = Interlocked.CompareExchange(ref completeState, CompletedFailure, NotCompleted); // try set failure
            Volatile.Write(ref error, result.Exception);      // set failure immmediately(but not locked).
        }

        switch (field)
        {
            case NotCompleted:
                return ResultStatus.Done;
            case CompletedSuccess:
                return ResultStatus.AlreadySuccess;
            case CompletedFailure:
                return ResultStatus.AlreadyFailed;
            case Disposed:
                ThrowObjectDiposedException();
                break;
        }

        return ResultStatus.Done; // not here.
    }

    public bool TrySetDisposed(out bool alreadyCompleted)
    {
        var field = Interlocked.Exchange(ref completeState, Disposed);
        switch (field)
        {
            case NotCompleted:
                alreadyCompleted = false;
                return true;
            case CompletedSuccess:
            case CompletedFailure:
                alreadyCompleted = true;
                return true;
            case Disposed:
                break;
        }

        alreadyCompleted = false;
        return false;
    }

    public bool IsCompleted
    {
        get
        {
            var currentState = Volatile.Read(ref completeState);
            switch (currentState)
            {
                case NotCompleted:
                    return false;
                case CompletedSuccess:
                    return true;
                case CompletedFailure:
                    return true;
                case Disposed:
                    ThrowObjectDiposedException();
                    break;
            }

            return false; // not here.
        }
    }

    public bool IsDisposed => Volatile.Read(ref completeState) == Disposed;

    public Result? TryGetResult()
    {
        var currentState = Volatile.Read(ref completeState);

        switch (currentState)
        {
            case NotCompleted:
                return null;
            case CompletedSuccess:
                return Result.Success;
            case CompletedFailure:
                return Result.Failure(GetException());
            case Disposed:
                ThrowObjectDiposedException();
                break;
        }

        return null; // not here.
    }

    // be careful to use, this method need to call after ResultStatus.AlreadyFailed.
    Exception GetException()
    {
        Exception? error;
        do
        {
            error = Volatile.Read(ref this.error);
        } while (error == null); // failure is set after completeState is failure, so need to loop.
        return error;
    }

    static void ThrowObjectDiposedException()
    {
        throw new ObjectDisposedException("");
    }
}
