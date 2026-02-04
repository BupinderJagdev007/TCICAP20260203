namespace TPICAP;

//  PROBLEM: Implement the IOrder interface
//
//  1.    The class should take IOrderService & a decimal threshold value as parameters in the constructor
//  2.    In RespondToTick if the incoming price is less than the threshold use the IOrderService to buy, and also raise the "Placed" event
//  3.    If anything goes wrong you should raise the "Errored" event
//  4.    Prevent any further buys once one has been placed, or if there is an error
//  5.    The code should be thread safe, and you should assume it can be called from multiple threads

//  You should upload all your code as Visual Studio solution to GitHub and send the URL to your submission
//  You should also implement any tests you would write for this, you may use whatever framework you like for this

public interface IOrder : IPlaced, IErrored
{
    void RespondToTick(string code, decimal price);
}

public interface IOrderService
{
    void Buy(string code, int quantity, decimal price);

    void Sell(string code, int quantity, decimal price);
}

public interface IPlaced
{
    event PlacedEventHandler Placed;
}

public delegate void PlacedEventHandler(PlacedEventArgs e);

public class PlacedEventArgs
{
    public PlacedEventArgs(string code, decimal price)
    {
        Code = code;
        Price = price;
    }

    public string Code { get; }

    public decimal Price { get; }
}

public interface IErrored
{
    event ErroredEventHandler Errored;
}

public delegate void ErroredEventHandler(ErroredEventArgs e);

public class ErroredEventArgs : ErrorEventArgs
{
    public ErroredEventArgs(string code, decimal price, Exception ex) : base(ex)
    {
        Code = code;
        Price = price;
    }

    public string Code { get; }

    public decimal Price { get; }
}