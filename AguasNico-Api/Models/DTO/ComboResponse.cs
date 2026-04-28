namespace AguasNico_Api.Models.DTO;

public class ComboResponse
{
    public List<Item> Items { get; set; } = [];

    public class Item
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}

