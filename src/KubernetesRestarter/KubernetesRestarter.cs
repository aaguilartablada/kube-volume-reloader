namespace Kubevolumereloader;

public class KubernetesRestarter : IKubernetesRestarter
{
  private IKubernetes _client;
  private IConfiguration _config;
  private JsonSerializerOptions _options;

  public KubernetesRestarter(IKubernetes client, IConfiguration config)
  {
    _client = client;
    _config = config;
    _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
  }

  public async Task RestartDeploymentAsync(string name, string @namespace, string secretcmName)
  {
    // Getting and checking if deployment exists
    var deployment = await _client.ReadNamespacedDeploymentAsync(name, @namespace);
    if (deployment == null) return;

    // Checking if the secret or configmap is actually mounted in the pods. If not, we do NOT send the restart signal
    if (!MustRestart(deployment.Spec.Template.Spec.Volumes, secretcmName)) return;

    var oldObject = JsonSerializer.SerializeToDocument(deployment, _options);

    // Getting annotations and initialize them if they do NOT exist
    var annotations = deployment.Spec.Template.Metadata.Annotations;
    if (annotations == null)
      annotations = new Dictionary<string, string>();

    // Creating or updating annotation to restart the deloyment
    annotations[_config["RestartAnnotation"]] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
    deployment.Spec.Template.Metadata.Annotations = annotations;

    // Creating and patching the new deployment
    var expectedObject = JsonSerializer.SerializeToDocument(deployment);
    var patch = oldObject.CreatePatch(expectedObject);
    await _client.PatchNamespacedDeploymentAsync(new V1Patch(patch, V1Patch.PatchType.JsonPatch), name, @namespace);
  }

  public async Task RestartStatefulsetAsync(string name, string @namespace, string secretcmName)
  {
    // Getting and checking if statefulset exists
    var statefulset = await _client.ReadNamespacedStatefulSetAsync(name, @namespace);
    if (statefulset == null) return;

    // Checking if the secret or configmap is actually mounted in the pods. If not, we do NOT send the restart signal
    if (!MustRestart(statefulset.Spec.Template.Spec.Volumes, secretcmName)) return;

    var oldObject = JsonSerializer.SerializeToDocument(statefulset, _options);

    // Getting annotations and initialize them if they do NOT exist
    var annotations = statefulset.Spec.Template.Metadata.Annotations;
    if (annotations == null)
      annotations = new Dictionary<string, string>();

    // Creating or updating annotation to restart the statefulset
    annotations[_config["RestartAnnotation"]] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
    statefulset.Spec.Template.Metadata.Annotations = annotations;

    // Creating and patching the new statefulset
    var expectedObject = JsonSerializer.SerializeToDocument(statefulset);
    var patch = oldObject.CreatePatch(expectedObject);
    await _client.PatchNamespacedStatefulSetAsync(new V1Patch(patch, V1Patch.PatchType.JsonPatch), name, @namespace);
  }

  public async Task RestartDaemonsetAsync(string name, string @namespace, string secretcmName)
  {
    // Getting and checking if daemonset exists
    var daemonset = await _client.ReadNamespacedDaemonSetAsync(name, @namespace);
    if (daemonset == null) return;

    // Checking if the secret or configmap is actually mounted in the pods. If not, we do NOT send the restart signal
    if (!MustRestart(daemonset.Spec.Template.Spec.Volumes, secretcmName)) return;

    var oldObject = JsonSerializer.SerializeToDocument(daemonset, _options);

    // Getting annotations and initialize them if they do NOT exist
    var annotations = daemonset.Spec.Template.Metadata.Annotations;
    if (annotations == null)
      annotations = new Dictionary<string, string>();

    // Creating or updating annotation to restart the deloyment
    annotations[_config["RestartAnnotation"]] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
    daemonset.Spec.Template.Metadata.Annotations = annotations;

    // Creating and patching the new daemonset
    var expectedObject = JsonSerializer.SerializeToDocument(daemonset);
    var patch = oldObject.CreatePatch(expectedObject);
    await _client.PatchNamespacedDaemonSetAsync(new V1Patch(patch, V1Patch.PatchType.JsonPatch), name, @namespace);
  }

  private bool MustRestart(IList<V1Volume> volumeList, string secretcmName)
  {
    var restart = false;
    foreach (var v in volumeList)
    {
      if (v.Secret != null)
      {
        if (v.Secret.SecretName == secretcmName)
        {
          restart = true;
          break;
        }
      }
      if (v.ConfigMap != null)
      {
        if (v.ConfigMap.Name == secretcmName)
        {
          restart = true;
          break;
        }
      }
    }
    return restart;
  }

}