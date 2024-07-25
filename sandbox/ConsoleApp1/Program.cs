using R3;




var subject = new Subject<int>();

subject.Subscribe(Subscribe2);

subject.OnNext(1);
subject.OnNext(2);

void Subscribe2(int y)
{
    Console.WriteLine(y);
    subject.Subscribe(Subscribe2);
}



