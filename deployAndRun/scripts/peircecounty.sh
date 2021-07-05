IMAGE=nagareddy.reddy-D-201712032309
CONFIG=/app/bin/estatepierce.yaml
docker run -d docker.io/nagareddy/estateai:$IMAGE /app/bin/estateAIApp --config=$CONFIG

