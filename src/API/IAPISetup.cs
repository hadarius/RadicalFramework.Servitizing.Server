namespace Radical.Servitizing.Server.API
{
    public interface IAPISetup
    {

        IAPISetup UseHeaderForwarding();
        IAPISetup UseStandardSetup(string[] apiVersions);

        IAPISetup UseDataServices();

        IAPISetup UseExternalProvider();

        IAPISetup UseInternalProvider();

        IAPISetup UseDataMigrations();

        IAPISetup UseJwtUserInfo();

        IAPISetup UseSwaggerSetup(string[] apiVersions);
    }
}