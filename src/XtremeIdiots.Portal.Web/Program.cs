namespace XtremeIdiots.Portal.Web;

/// <summary>
/// Entry point Web application
/// </summary>
public class Program
{
 /// <summary>
 /// Application entry point
 /// </summary>
 /// <param name="args">Command line arguments</param>
 public static void Main(string[] args)
 {
 CreateHostBuilder(args).Build().Run();
 }

 /// <summary>
 /// Creates and configures the host builder with Startup class pattern
 /// </summary>
 /// <param name="args">Command line arguments</param>
 /// <returns>Configured host builder</returns>
 public static IHostBuilder CreateHostBuilder(string[] args)
 {
 return Host.CreateDefaultBuilder(args)
 .ConfigureWebHostDefaults(webBuilder =>
 {
 webBuilder.UseStartup<Startup>();
 });
 }
}