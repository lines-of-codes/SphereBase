using SphereBase.Client;

Client client = new();
Console.WriteLine(client.Set("h", "69"));
Console.WriteLine(client.Get("h"));
