# kube-volume-reloader
A little kubernetes operator to restart Deployments, Statefulsets and/or Daemonsets when a referenced ConfigMap or Secret has changed

It is based on a ValidatingWebhook that is notified with Secrets and ConfigMap updates. If a Secret or ConfigMap contains a concrete annotation the process to restart Pods is started.

# Annotation

## Name

The default name for the annotation is 'kube-volume-reloader', but it can be changed by the setting 'Annotation'.

## Format

The value for the annotation is a comma-separated list of deployments, statefulsets and/or daemonsets to be restarted. Each has to be indicated in the format '<kind>/<name>'. For example, let's suppose we want to restart the deployment 'my-app' and the statefulset 'my-db' when the secret 'my-secret' changes:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: example-api-certificate
  annotations:
    kube-volume-reloader: "deployment/my-app,statefulset/my-db"
type: opaque
stringData:
  mysecret: topsecret
```