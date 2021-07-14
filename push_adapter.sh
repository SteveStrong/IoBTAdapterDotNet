#!/bin/bash

# build docker file and push to azure
# docker system prune -a


# docker login iobtweb.azurecr.io
# username iobtweb
# password  (get it from azure access keys to azurecr)

#  https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-docker-cli
#  to put into azurecr.io 
#  az login
#  az acr login --name iobtweb

docker build -t iobtmesh -f Dockerfile  .
echo "build done"

docker tag iobtmesh laweb700.azurecr.io/iobtmesh341
echo "tag done"
docker push laweb700.azurecr.io/iobtmesh341

echo "push done"
