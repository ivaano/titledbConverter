namespace titledbConverter.Services.Interface;

public interface IDbInitializationService
{
    Task InitializeAsync(bool dropDatabase);
}