using System;

namespace CourierService.Model;

public class Courier
{
    public int Id { get; set;}
    public required string Name { get; set;}
    public int District { get; set;}
}
