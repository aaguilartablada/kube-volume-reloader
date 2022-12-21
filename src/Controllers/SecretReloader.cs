namespace Kubevolumereloader.Controllers;

public class SecretReloader : IValidationWebhook<V1Secret>
{
  public AdmissionOperations Operations => AdmissionOperations.Update;
  private IKubernetes _client;
  private IKubernetesRestarter _restarter;
  private IConfiguration _config;

  public SecretReloader(IKubernetes client, IKubernetesRestarter restarter, IConfiguration config)
  {
    _client = client;
    _restarter = restarter;
    _config = config;
  }

  public ValidationResult Update(V1Secret oldSecret, V1Secret newSecret, bool dryRun)
  {
    // if oldSecret is null it means that it's a new secret, so we do NOT have to reload anything
    if (oldSecret == null) return ValidationResult.Success();

    var reloadAnnotationValue = Utils.GetAnnotation(newSecret.Metadata.Annotations, _config["Annotation"]);

    // if secret does not have annotations or annotations does not contains our annotation we have nothing to reload
    if (string.IsNullOrEmpty(reloadAnnotationValue)) return ValidationResult.Success();

    // if data for old and new secrets are the same there are no changes to reload
    if (Utils.CheckDictionaryEquality(oldSecret.Data, newSecret.Data)) return ValidationResult.Success();

    var objectsToRestart = Utils.SplitAnnotation(reloadAnnotationValue);

    var taskList = new List<Task>();

    var name = newSecret.Metadata.Name;
    var @namespace = newSecret.Metadata.NamespaceProperty;
    foreach (var deployment in objectsToRestart["deployment"])
    {
      taskList.Add(_restarter.RestartDeploymentAsync(deployment, @namespace, name));
    }
    foreach (var statefulset in objectsToRestart["statefulset"])
    {
      taskList.Add(_restarter.RestartStatefulsetAsync(statefulset, @namespace, name));
    }
    foreach (var daemonset in objectsToRestart["daemonset"])
    {
      taskList.Add(_restarter.RestartDaemonsetAsync(daemonset, @namespace, name));
    }

    return ValidationResult.Success();
  }

}