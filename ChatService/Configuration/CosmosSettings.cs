namespace ChatService.Configuration;

public record CosmosSettings
{
    public string ConnectionString { get; init; } =
        "AccountEndpoint=https://chatservice-jadoueida.documents.azure.com:443/;AccountKey=z5EcWudEqttMSOwK01HepJ1YrXWcUpgraFu8aIBSRXb1Mc22xFH16eckDzJdCf8J4luynfaoRrDKACDbPNhJlw==;";

}