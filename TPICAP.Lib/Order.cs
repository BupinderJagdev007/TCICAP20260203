namespace TPICAP;
public class Order : IOrder
{
    private object _lockObject = new object();
    private bool InstanceUsed { get; set; } = false;
    private IOrderService OrderService { get; set; }
    private decimal Threshold { get; set; }

    public event PlacedEventHandler Placed = delegate { };
    public event ErroredEventHandler Errored = delegate { };
    public int Quantity { get; set; } = 100;
    public Order(IOrderService orderService, decimal threshold)
    {
        OrderService = orderService;
        Threshold = threshold;
    }

    public void RespondToTick(string code, decimal price)
    {
        if (InstanceUsed)
            return; // prevent reuse on single use or error, assumed silently

        Func<bool> commandToExecute = (price < Threshold) ?
            () => { OrderService.Buy(code, Quantity, price); return true; }
        :
// if selling was to be considered, as its not in test exam spec, swap this implementaton
// implied sell if price>=Threshold, its not been specifed by the exam spec but fill in a working protoype logically!
            //() => { OrderService.Sell(code, Quantity, price); return true; };
// as per spec wont ever sell.
            () => { return false; };
        try
        {            
             lock (_lockObject)
             {
                // re check instance was not used between InstanceUsed check above and lock due to multi threading
                if (InstanceUsed) 
                    return;  // prevent reuse on single use or error, assumed silently

                if (!commandToExecute.Invoke())
                    return;

                Placed.Invoke(new PlacedEventArgs(code, price));

                InstanceUsed = true;                
            } 
        }
        // If anything goes wrong trigger event
        catch (Exception ex)
        {
            var eea = new ErroredEventArgs(code, price, ex);

            Errored.Invoke(eea);

            InstanceUsed = true;
        }
    }
}
