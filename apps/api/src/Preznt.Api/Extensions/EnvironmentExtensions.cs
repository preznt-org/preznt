namespace Preznt.Api.Extensions;

public static class EnvironmentExtensions
{
    /// <summary>
    /// Loads .env file by walking up from the base directory until found.
    /// This supports running from bin/Debug/net9.0 while .env is at solution root.
    /// </summary>
    public static void LoadDotEnv()
    {
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        
        while (currentDir != null)
        {
            var envPath = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
                return;
            }
            currentDir = currentDir.Parent;
        }
    }
}
