IMAGE=nagareddy.reddy-D-201712032309
CONFIG=/app/bin/estatesnohomish.yaml
docker run -d docker.io/nagareddy/estateai:$IMAGE /app/bin/estateAIApp --config=$CONFIG

