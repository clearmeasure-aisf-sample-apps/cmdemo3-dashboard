# The dashboard as a container image, for a system whose runtime serves it from a cluster (runtime aks-argocd): the
# published site (./site, the Build's artifact dashboard-site) behind nginx, without root and on port 8080. The image
# is the same in every environment; what an environment's page shows (topology.json and runtime/) is mounted at
# /content by the cluster (nginx.conf).
FROM nginxinc/nginx-unprivileged:1.29-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY site/ /usr/share/nginx/html/
