Local TLS Setup for hbg.local Domains with cert‑manager
Prerequisites
Docker Desktop with Kubernetes enabled.
kubectl installed and configured.
Helm installed.
Administrative access to edit your /etc/hosts file.
Access to your system’s certificate trust store (e.g., macOS Keychain) for adding your CA certificate.
1. Install cert‑manager
Install cert‑manager in its own namespace using Helm:

bash
Copy
# Add the Jetstack repository
helm repo add jetstack https://charts.jetstack.io
helm repo update

# Create the cert-manager namespace and install CRDs
kubectl create namespace cert-manager
kubectl apply -f https://github.com/jetstack/cert-manager/releases/latest/download/cert-manager.crds.yaml

# Install cert-manager (adjust the version if needed)
helm install cert-manager jetstack/cert-manager --namespace cert-manager --version v1.11.0
Verify cert‑manager pods are running:

bash
Copy
kubectl get pods -n cert-manager
2. Set Up a Local CA
Since Let’s Encrypt does not issue certificates for local domains, we create a self‑signed CA for local development.

a. Create a Self‑Signed ClusterIssuer
Create a file named selfsigned-issuer.yaml with the following content:

yaml
Copy
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: selfsigned-issuer
spec:
  selfSigned: {}
Apply it:

bash
Copy
kubectl apply -f selfsigned-issuer.yaml
b. Issue a CA Certificate
Create a CA certificate that will be used to sign your application certificates. Create a file named hbg-selfsigned-ca.yaml with the content below:

yaml
Copy
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: hbg-selfsigned-ca
  namespace: cert-manager
spec:
  isCA: true
  commonName: hbg-selfsigned-ca
  secretName: hbg-selfsigned-ca-secret
  duration: 87600h  # ~10 years
  subject:
    organizations:
      - HBG GmbH
    organizationalUnits:
      - LocalDeployment
  privateKey:
    algorithm: ECDSA
    size: 256
  issuerRef:
    name: selfsigned-issuer
    kind: ClusterIssuer
    group: cert-manager.io
Apply it:

bash
Copy
kubectl apply -f hbg-selfsigned-ca.yaml
c. Create a CA ClusterIssuer Using the CA Certificate
Now, create a ClusterIssuer that uses your new CA certificate. Create a file named hbg-ca-issuer.yaml with the following content:

yaml
Copy
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: hbg-ca-issuer
spec:
  ca:
    secretName: hbg-selfsigned-ca-secret
Apply it:

bash
Copy
kubectl apply -f hbg-ca-issuer.yaml
3. Issue a TLS Certificate for hbg.local Domains
Create a certificate that covers sts.hbg.local, admin.hbg.local, and spa.hbg.local. Create a file named hbg-cert.yaml with the content below:

yaml
Copy
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: hbg-cert
  namespace: default
spec:
  secretName: hbg-tls-secret  # This secret will contain your TLS certificate and key
  issuerRef:
    name: hbg-ca-issuer
    kind: ClusterIssuer
  commonName: sts.hbg.local
  dnsNames:
    - "sts.hbg.local"
    - "admin.hbg.local"
    - "spa.hbg.local"
  duration: 2160h  # 90 days validity (adjust if needed)
Apply it:

bash
Copy
kubectl apply -f hbg-cert.yaml
Cert‑manager will issue the certificate and store the key and certificate in the secret hbg-tls-secret in the default namespace.

4. Configure Ingress (Optional)
If you wish to expose your services via Ingress, create an Ingress resource that uses the TLS certificate. For example, if you have services named sts-service, admin-service, and spa-service running on port 80 in the default namespace, create a file named hbg-ingress.yaml:

yaml
Copy
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: hbg-ingress
  namespace: default
  annotations:
    cert-manager.io/cluster-issuer: "hbg-ca-issuer"
spec:
  tls:
    - hosts:
        - sts.hbg.local
        - admin.hbg.local
        - spa.hbg.local
      secretName: hbg-tls-secret
  rules:
    - host: sts.hbg.local
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: sts-service
                port:
                  number: 80
    - host: admin.hbg.local
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: admin-service
                port:
                  number: 80
    - host: spa.hbg.local
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: spa-service
                port:
                  number: 80
Apply the Ingress resource:

bash
Copy
kubectl apply -f hbg-ingress.yaml
Note: An Ingress controller (e.g., NGINX Ingress) must be installed in your cluster to process these resources.

5. Update Your Hosts File & Trust the Local CA Certificate
a. Update Your /etc/hosts File
For your local machine to resolve the domains, add the following entry to your /etc/hosts file:

plaintext
Copy
127.0.0.1   sts.hbg.local admin.hbg.local spa.hbg.local
b. Extract and Trust the CA Certificate
Extract the CA certificate from Kubernetes:

bash
Copy
kubectl get secret hbg-selfsigned-ca-secret -n cert-manager \
  -o jsonpath="{.data['tls\.crt']}" | base64 --decode > hbg-selfsigned-ca.crt
Then, add the hbg-selfsigned-ca.crt file to your system’s trusted certificate store:

macOS:
Open Keychain Access, drag and drop the hbg-selfsigned-ca.crt file into the System or login keychain, double-click the imported certificate, expand the Trust section, and set When using this certificate to Always Trust.
Other Operating Systems:
Follow your OS-specific instructions to add a trusted CA.
6. Verify Your Setup
Check the Certificate Status:

Verify that the certificate has been issued and the secret created:

bash
Copy
kubectl describe certificate hbg-cert -n default
Test in Your Browser:

Open your browser and navigate to:

https://sts.hbg.local
https://admin.hbg.local
https://spa.hbg.local
Each domain should now load over HTTPS with a certificate issued by your trusted local CA.

Troubleshooting
Certificate Issuance Issues:
If the certificate status remains in the "Issuing" phase for too long, inspect the CertificateRequest resources or check cert‑manager logs:

bash
Copy
kubectl logs -n cert-manager deploy/cert-manager
Ingress Not Functioning:
Ensure an Ingress controller is installed and configured if you’re using Ingress resources.