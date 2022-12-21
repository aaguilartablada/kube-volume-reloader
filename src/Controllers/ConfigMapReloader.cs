namespace Kubevolumereloader.Controllers;

public class ConfigMapReloader : IValidationWebhook<V1ConfigMap>
{
  public AdmissionOperations Operations => AdmissionOperations.Update;
  private IKubernetes _client;
  private IKubernetesRestarter _restarter;
  private IConfiguration _config;

  public ConfigMapReloader(IKubernetes client, IKubernetesRestarter restarter, IConfiguration config)
  {
    _client = client;
    _restarter = restarter;
    _config = config;
  }

  public ValidationResult Update(V1ConfigMap oldCm, V1ConfigMap newCm, bool dryRun)
  {
    // if oldCm is null it means that it's a new secret, so we do NOT have to reload anything
    if (oldCm == null) return ValidationResult.Success();

    var reloadAnnotationValue = Utils.GetAnnotation(newCm.Metadata.Annotations, _config["Annotation"]);

    // if cm does not have annotations or annotations does not contains our annotation we have nothing to reload
    if (string.IsNullOrEmpty(reloadAnnotationValue)) return ValidationResult.Success();

    // if data for old and new secrets are the same there are no changes to reload
    if (Utils.CheckDictionaryEquality(oldCm.Data, newCm.Data)) return ValidationResult.Success();

    var objectsToRestart = Utils.SplitAnnotation(reloadAnnotationValue);

    var taskList = new List<Task>();

    var name = newCm.Metadata.Name;
    var @namespace = newCm.Metadata.NamespaceProperty;
    foreach(var deployment in objectsToRestart["deployment"])
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