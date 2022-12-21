namespace Kubevolumereloader;

public interface IKubernetesRestarter
{
  Task RestartDeploymentAsync(string name, string @namespace, string secretcmName);
  Task RestartStatefulsetAsync(string name, string @namespace, string secretcmName);
  Task RestartDaemonsetAsync(string name, string @namespace, string secretcmName);
}