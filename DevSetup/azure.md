
Docker Setup 
------------
1. curl -fsSL get.docker.com -o get-docker.sh
2. sudo sh get-docker.sh


Mini Kube setup
---------------

Installing Mini Kube : https://kubernetes.io/docs/tasks/tools/install-minikube/

curl -Lo minikube https://storage.googleapis.com/minikube/releases/v0.24.1/minikube-linux-amd64 && chmod +x minikube && sudo mv minikube /usr/local/bin/

Install Kubectl 

curl -LO https://storage.googleapis.com/kubernetes-release/release/$(curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt)/bin/linux/amd64/kubectl

curl -LO https://storage.googleapis.com/kubernetes-release/release/v1.8.0/bin/linux/amd64/kubectl

chmod +x ./kubectl
sudo mv ./kubectl /usr/local/bin/kubectl


start mini kube with no vm driver

minikube start --vm-driver=none


change permissions

.kube ( sudo chmod 644 ~/.kube/*)
 .minikube  ( sudo chmod 644 ~/.kube/*)


