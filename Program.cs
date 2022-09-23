partial class Program
{
    static CircularBuffer CircularBuffer;
    static void Main(string[] args)
    {
        File.WriteAllText("letters.txt", String.Empty);
        File.WriteAllText("digits.txt", String.Empty);
        File.WriteAllText("other.txt", String.Empty);

        CircularBuffer = new CircularBuffer(10);
        var producer = new Task(() => Producer());
        var consumer1 = new Task(() => Consumer(1));
        var consumer2 = new Task(() => Consumer(2));
        var consumer3 = new Task(() => Consumer(3));

        producer.Start();
        consumer1.Start();
        consumer2.Start();
        consumer3.Start();

        Task.WaitAll(producer);
    }
    static void Producer()
    {
        while (true)
        {
            var symbol = Console.ReadKey().KeyChar;

            CircularBuffer.WatchEmpty.WaitOne();
            CircularBuffer.WatchAccess.WaitOne();

            CircularBuffer.InnerBuffer[CircularBuffer.Head] = symbol;
            CircularBuffer.Head = (CircularBuffer.Head + 1) % CircularBuffer.Lenght;

            CircularBuffer.WatchAccess.Release();
            CircularBuffer.WatchFull.Release();
        }
    }
    static void Consumer(int symbolType)
    {
        while (true)
        {
            CircularBuffer.WatchFull.WaitOne();
            CircularBuffer.WatchAccess.WaitOne();

            char symbol = CircularBuffer.InnerBuffer[CircularBuffer.Tail];

            switch (symbolType)
            {
                case 1:
                    if (char.IsLetter(symbol))
                    {
                        CircularBuffer.Tail = (CircularBuffer.Tail + 1) % CircularBuffer.Lenght;

                        using (StreamWriter writer = new StreamWriter("letters.txt", true))
                        {
                            writer.Write(symbol);
                        }

                        CircularBuffer.WatchAccess.Release();
                        CircularBuffer.WatchEmpty.Release();
                    }
                    else
                    {
                        CircularBuffer.WatchFull.Release();
                        CircularBuffer.WatchAccess.Release();
                    }
                    break;
                case 2:
                    if (char.IsDigit(symbol))
                    {
                        CircularBuffer.Tail = (CircularBuffer.Tail + 1) % CircularBuffer.Lenght;

                        using (StreamWriter writer = new StreamWriter("digits.txt", true))
                        {
                            writer.Write(symbol);
                        }

                        CircularBuffer.WatchAccess.Release();
                        CircularBuffer.WatchEmpty.Release();
                    }
                    else
                    {
                        CircularBuffer.WatchFull.Release();
                        CircularBuffer.WatchAccess.Release();
                    }
                    break;
                case 3:
                    if (!char.IsLetterOrDigit(symbol))
                    {
                        CircularBuffer.Tail = (CircularBuffer.Tail + 1) % CircularBuffer.Lenght;

                        using (StreamWriter writer = new StreamWriter("other.txt", true))
                        {
                            writer.Write(symbol);
                        }

                        CircularBuffer.WatchAccess.Release();
                        CircularBuffer.WatchEmpty.Release();
                    }
                    else
                    {
                        CircularBuffer.WatchFull.Release();
                        CircularBuffer.WatchAccess.Release();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
class CircularBuffer
{
    public char[] InnerBuffer { get; set; }
    public int Head { get; set; }
    public int Tail { get; set; }
    public int Lenght { get { return InnerBuffer.Length; } }
    public Semaphore WatchAccess { get; set; }
    public Semaphore WatchFull { get; set; }
    public Semaphore WatchEmpty { get; set; }

    public CircularBuffer(int size)
    {
        InnerBuffer = new char[size];
        Head = 0;
        Tail = 0;
        WatchAccess = new Semaphore(1, 1);
        WatchFull = new Semaphore(0, size);
        WatchEmpty = new Semaphore(size, size);
    }
}


