namespace Kubevolumereloader;

public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    services
        .AddKubernetesOperator()

#if DEBUG
    .AddWebhookLocaltunnel();
    Console.WriteLine("Debug version");
#endif

    services.AddSingleton<IKubernetes>(new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
    services.AddSingleton<IKubernetesRestarter, KubernetesRestarter>();
  }

  public void Configure(IApplicationBuilder app)
  {
    app.UseKubernetesOperator();
  }
}