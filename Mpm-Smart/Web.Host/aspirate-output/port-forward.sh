#!/bin/bash

declare -A services=(
  [web-server]="8001:8080"
  [api]="8002:8080"
  [aspire-dashboard]="8003:18888"
  [dbmanager]="8004:8080"
  [maildev]="8005:9324"
  [neo4j]="8006:9326"
  [datagateway]="8007:8080"
  [sql]="8008:1433"
)

for service in "${!services[@]}"; do
  port=${services[$service]}
  echo "Port-forwarding for $service on ports $port"
  kubectl port-forward -n mpm-smart svc/$service $port &
done

wait