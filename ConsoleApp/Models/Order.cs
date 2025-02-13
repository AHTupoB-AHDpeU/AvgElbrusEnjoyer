public class Order
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; }

    public Client Client { get; set; }
}
